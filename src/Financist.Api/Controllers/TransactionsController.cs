using Asp.Versioning;
using Financist.Application.Features.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Financist.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TransactionDto>>> List(CancellationToken cancellationToken)
    {
        var transactions = await _transactionService.ListAsync(cancellationToken);
        return Ok(transactions);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> Create([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionService.CreateAsync(request, cancellationToken);
        return Created($"/api/v1/transactions/{transaction.Id}", transaction);
    }
}
