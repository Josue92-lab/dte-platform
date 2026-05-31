namespace DTE.Domain.Primitives;

public sealed record ValidationError(string Code, string Message, string? Field = null);
