using NUnit.Framework;
using SalesManagementPlatform.Domain.Entities;
using SalesManagementPlatform.Domain.Exceptions;
using SalesManagementPlatform.Domain.ValueTypes;

namespace SalesManagementPlatform.Tests.Unit.Domain;

[Category("Unit")]
public class OrderRuleTests
{
    [TestCase(0)]
    [TestCase(-1)]
    public void LineItem_RejectsNonPositiveQuantity(int quantity)
    {
        Assert.That(() => new LineItem("SKU", "Sample", quantity, 1m),
            Throws.TypeOf<DomainRuleException>().With.Message.Contains("quantity"));
    }

    [Test]
    public void LineItem_RejectsNegativePrice()
    {
        Assert.That(() => new LineItem("SKU", "Sample", 1, -0.01m),
            Throws.TypeOf<DomainRuleException>().With.Message.Contains("price"));
    }

    [Test]
    public void FreeItem_HasZeroLineTotal()
    {
        var item = new LineItem("GIFT", "Free sample", 1, 0m);
        Assert.That(item.LineTotal, Is.Zero);
    }

    [Test]
    public void Order_CalculatesLineTotalsSubtotalAndTotalFromQuantitiesAndPrices()
    {
        var order = NewOrder();
        var first = new LineItem("A", "Notebook", 3, 12.50m);
        var second = new LineItem("B", "Pencil", 2, 1.25m);
        order.LineItems.AddRange([first, second]);

        Assert.That(first.LineTotal, Is.EqualTo(37.50m));
        Assert.That(second.LineTotal, Is.EqualTo(2.50m));
        Assert.That(order.OrderTotals.Subtotal, Is.EqualTo(40m));
        Assert.That(order.OrderTotals.Total, Is.EqualTo(44m));
    }

    [Test]
    public void PendingOrder_CanBeConfirmedAndThenFulfilled()
    {
        var order = NewOrder();
        order.ChangeOrderStatus(OrderStatus.Confirmed);
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Confirmed));
        Assert.That(order.CompletedDateTimeStamp, Is.Null);

        order.ChangeOrderStatus(OrderStatus.Fulfilled);
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Fulfilled));
        Assert.That(order.CompletedDateTimeStamp, Is.Not.Null);
    }

    [TestCase(OrderStatus.Fulfilled)]
    [TestCase(OrderStatus.Cancelled)]
    public void TerminalOrder_RejectsFurtherChangesAndExplainsWhy(OrderStatus terminalStatus)
    {
        var order = NewOrder();
        order.ChangeOrderStatus(terminalStatus);
        Assert.That(() => order.ChangeOrderStatus(OrderStatus.Pending),
            Throws.TypeOf<DomainRuleException>().With.Message.Contains(terminalStatus.ToString()).IgnoreCase);
        Assert.That(order.Status, Is.EqualTo(terminalStatus));
    }

    private static Order NewOrder() =>
        new("REF-1", DateTime.UtcNow, null, OrderStatus.Pending, "ZAR", "", Guid.NewGuid());
}
