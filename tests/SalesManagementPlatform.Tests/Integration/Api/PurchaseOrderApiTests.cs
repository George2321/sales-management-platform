using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using SalesManagementPlatform.Api.Controllers;
using SalesManagementPlatform.Application.Features.Orders.Dtos;
using SalesManagementPlatform.Application.Features.Orders.Interfaces;
using SalesManagementPlatform.Domain.Entities;
using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Tests.Integration.Api;

[Category("Integration")]
public class PurchaseOrderApiTests
{
    private const string OrdersUrl = "/api/PurchaseOrder";
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"order-tests-{Guid.NewGuid():N}.db");
    private OrderApiFactory _factory = null!;
    private HttpClient _client = null!;
    private Guid _customerId;

    [OneTimeSetUp]
    public async Task StartApi()
    {
        _factory = new OrderApiFactory(_databasePath);
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        Assert.That(File.Exists(_databasePath), Is.True, "The API must initialize the isolated test database.");
        var customers = await _client.GetFromJsonAsync<OrderCustomerOptionDto[]>(OrdersUrl + "/customers");
        _customerId = customers!.First().Id;
    }

    [OneTimeTearDown]
    public async Task StopApi()
    {
        _client?.Dispose();
        await _factory.DisposeAsync();
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath, ForeignKeys = true
        }.ToString());
        SqliteConnection.ClearPool(connection);
        File.Delete(_databasePath);
    }

    [Test]
    public async Task DuplicateReference_ReturnsConflictAndKeepsOriginalOrder()
    {
        const string reference = "TEST-REPEAT";
        using var first = await PostOrder(reference, new { sku = "A", name = "Original", quantity = 2, unitPrice = 4m });
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var original = await first.Content.ReadFromJsonAsync<OrderDetailsDto>();

        using var repeat = await PostOrder(reference, new { sku = "B", name = "Replacement", quantity = 1, unitPrice = 20m });
        Assert.That(repeat.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        var problem = await repeat.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Does.Contain("reference already exists"));

        var orders = await _client.GetFromJsonAsync<OrderListItemDto[]>(OrdersUrl);
        var saved = orders!.Single(order => order.ClientProvidedExternalReference == reference);
        Assert.That(saved.Id, Is.EqualTo(original!.Id));
        Assert.That(saved.LineItemCount, Is.EqualTo(1));
        Assert.That(saved.Subtotal, Is.EqualTo(8m));
    }

    [Test]
    public async Task Totals_AreCalculatedByServerEvenWhenClientSuppliesAmounts()
    {
        const string reference = "TEST-TOTALS";
        using var response = await _client.PostAsJsonAsync(OrdersUrl, new
        {
            clientProvidedExternalReference = reference, customerId = _customerId, currency = "ZAR",
            subtotal = 999m, total = 999m,
            lineItems = new[]
            {
                new { sku = "A", name = "Sample", quantity = 3, unitPrice = 2.50m, lineTotal = 999m },
                new { sku = "B", name = "Second item", quantity = 2, unitPrice = 1.25m, lineTotal = 999m }
            }
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var orders = await _client.GetFromJsonAsync<OrderListItemDto[]>(OrdersUrl);
        var saved = orders!.Single(o => o.ClientProvidedExternalReference == reference);
        Assert.That(saved.Subtotal, Is.EqualTo(10m));
        Assert.That(saved.Total, Is.EqualTo(11m));

        var details = await _client.GetFromJsonAsync<OrderDetailsDto>($"{OrdersUrl}/{saved.Id}");
        Assert.That(details!.LineItems.Select(item => item.LineTotal), Is.EqualTo(new[] { 7.50m, 2.50m }));
        Assert.That(details.Subtotal, Is.EqualTo(saved.Subtotal));
        Assert.That(details.Total, Is.EqualTo(saved.Total));
    }

    [Test]
    public async Task CancelledOrder_ReturnsHelpfulErrorWhenReopened()
    {
        using var create = await PostOrder("TEST-CANCEL", new { sku = "A", name = "Sample", quantity = 1, unitPrice = 2m });
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var order = await create.Content.ReadFromJsonAsync<OrderDetailsDto>();
        var url = $"{OrdersUrl}/{order!.Id}/status";
        using var cancel = await _client.PatchAsJsonAsync(url, new { expectedStatus = OrderStatus.Pending, status = OrderStatus.Cancelled });
        Assert.That(cancel.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var reopen = await _client.PatchAsJsonAsync(url, new { expectedStatus = OrderStatus.Cancelled, status = OrderStatus.Pending });
        Assert.That(reopen.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        var problem = await reopen.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problem!.Detail, Does.Contain("already cancelled"));
    }

    [Test]
    public async Task FractionalQuantity_IsRejectedAtTheApiBoundary()
    {
        using var response = await PostOrder("TEST-FRACTION", new { sku = "A", name = "Sample", quantity = 1.5m, unitPrice = 2m });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ExplicitZeroPrice_IsAcceptedButMissingPriceIsRejected()
    {
        using var free = await PostOrder("TEST-FREE", new { sku = "A", name = "Gift", quantity = 1, unitPrice = 0m });
        Assert.That(free.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        using var missing = await PostOrder("TEST-MISSING", new { sku = "B", name = "Sample", quantity = 1 });
        Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task List_ReturnsNewestOrderDateFirstRegardlessOfInsertionOrder()
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        foreach (var day in new[] { 2, 3, 1 })
        {
            var date = new DateTime(2025, 1, day, 12, 0, 0, DateTimeKind.Utc);
            var order = new Order($"TEST-SORT-{day}", date, null, OrderStatus.Pending, "ZAR", "", _customerId);
            order.LineItems.Add(new LineItem("A", "Sample", 1, 2m));
            await repository.CreateAsync(order, CancellationToken.None);
        }

        var orders = await _client.GetFromJsonAsync<OrderListItemDto[]>(OrdersUrl);
        var sortedReferences = orders!.Select(o => o.ClientProvidedExternalReference)
            .Where(reference => reference.StartsWith("TEST-SORT-", StringComparison.Ordinal));
        Assert.That(sortedReferences, Is.EqualTo(new[] { "TEST-SORT-3", "TEST-SORT-2", "TEST-SORT-1" }));
    }

    private Task<HttpResponseMessage> PostOrder(string reference, object lineItem) =>
        _client.PostAsJsonAsync(OrdersUrl, new
        {
            clientProvidedExternalReference = reference, customerId = _customerId, currency = "ZAR",
            lineItems = new[] { lineItem }
        });

    private sealed class OrderApiFactory(string databasePath) : WebApplicationFactory<PurchaseOrderController>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:SqlitePath"] = databasePath,
                ["Database:SeedDemoData"] = "true"
            }));
            return base.CreateHost(builder);
        }
    }
}
