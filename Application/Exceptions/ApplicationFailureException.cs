namespace SalesManagementPlatform.Application.Exceptions;

public sealed class ApplicationFailureException(ApplicationFailureKind kind, string title, string message)
    : Exception(message)
{
    public ApplicationFailureKind Kind { get; } = kind;
    public string Title { get; } = title;

    public static ApplicationFailureException Validation(string message) =>
        new(ApplicationFailureKind.Validation, "Validation failed", message);
}
