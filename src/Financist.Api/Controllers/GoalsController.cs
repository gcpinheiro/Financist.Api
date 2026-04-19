using Asp.Versioning;
using Financist.Application.Features.Goals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Financist.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/goals")]
public sealed class GoalsController : ControllerBase
{
    private readonly IGoalService _goalService;

    public GoalsController(IGoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<GoalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GoalDto>>> List(CancellationToken cancellationToken)
    {
        var goals = await _goalService.ListAsync(cancellationToken);
        return Ok(goals);
    }

    [HttpPost]
    [ProducesResponseType(typeof(GoalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GoalDto>> Create([FromBody] CreateGoalRequest request, CancellationToken cancellationToken)
    {
        var goal = await _goalService.CreateAsync(request, cancellationToken);
        return Created($"/api/v1/goals/{goal.Id}", goal);
    }
}
