namespace OrderManagement.Api.Services;

public enum OrderErrorKind
{
    None,
    Validation,
    NotFound,
    Conflict,
}

public sealed record OrderResult<T>(T? Value, string? Error, OrderErrorKind Kind)
{
    public bool Success => Error is null;

    public static OrderResult<T> Ok(T value) => new(value, null, OrderErrorKind.None);

    public static OrderResult<T> Fail(string error, OrderErrorKind kind) => new(default, error, kind);
}
