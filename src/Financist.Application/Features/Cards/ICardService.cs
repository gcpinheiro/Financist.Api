namespace Financist.Application.Features.Cards;

public interface ICardService
{
    Task<CardDto> CreateAsync(CreateCardRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CardDto>> ListAsync(CancellationToken cancellationToken = default);
}
