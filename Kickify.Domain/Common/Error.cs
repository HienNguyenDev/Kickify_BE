namespace Kickify.Domain.Common;

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);

    public Error(string code, string description, ErrorType type, Dictionary<string, object>? metadata = null)
    {
        Code = code;
        Description = description;
        Type = type;
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }
    public Dictionary<string, object> Metadata { get; }

    public static Error Failure(string code, string description, Dictionary<string, object>? metadata = null) =>
        new(code, description, ErrorType.Failure, metadata);

    public static Error NotFound(string code, string description, Dictionary<string, object>? metadata = null) =>
        new(code, description, ErrorType.NotFound, metadata);

    public static Error Problem(string code, string description, Dictionary<string, object>? metadata = null) =>
        new(code, description, ErrorType.Problem, metadata);

    public static Error Conflict(string code, string description, Dictionary<string, object>? metadata = null) =>
        new(code, description, ErrorType.Conflict, metadata);

    public Error WithMetadata(string key, object value)
    {
        var newMetadata = new Dictionary<string, object>(Metadata) { [key] = value };
        return new Error(Code, Description, Type, newMetadata);
    }
}
