namespace Financist.Application.AI.Interfaces;

public interface IRagContextService
{
    Task<string?> BuildContextAsync(string message, CancellationToken cancellationToken = default);
}
