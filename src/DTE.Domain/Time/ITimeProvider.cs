namespace DTE.Domain.Time;

public interface ITimeProvider
{
    DateTime UtcNow { get; }
}
