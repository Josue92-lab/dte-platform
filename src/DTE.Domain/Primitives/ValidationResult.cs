namespace DTE.Domain.Primitives;

public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors = new();

    public IReadOnlyCollection<ValidationError> Errors => _errors.AsReadOnly();
    public bool IsValid => _errors.Count == 0;

    internal void AddError(ValidationError error) => _errors.Add(error);

    public static ValidationResult Valid() => new();
}
