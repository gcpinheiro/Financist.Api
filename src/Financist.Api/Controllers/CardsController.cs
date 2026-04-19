using Asp.Versioning;
using Financist.Application.Features.Cards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Financist.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/cards")]
public sealed class CardsController : ControllerBase
{
    private readonly ICardService _cardService;

    public CardsController(ICardService cardService)
    {
        _cardService = cardService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CardDto>>> List(CancellationToken cancellationToken)
    {
        var cards = await _cardService.ListAsync(cancellationToken);
        return Ok(cards);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CardDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CardDto>> Create([FromBody] CreateCardRequest request, CancellationToken cancellationToken)
    {
        var card = await _cardService.CreateAsync(request, cancellationToken);
        return Created($"/api/v1/cards/{card.Id}", card);
    }
}
