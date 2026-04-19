using Financist.Application.Abstractions.Persistence;
using Financist.Application.Abstractions.Services;
using Financist.Application.Common.Exceptions;
using Financist.Domain.Entities;
using Financist.Domain.ValueObjects;

namespace Financist.Application.Features.Goals;

public sealed class GoalService : IGoalService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GoalService(
        IGoalRepository goalRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<GoalDto> CreateAsync(CreateGoalRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["name"] = ["Goal name is required."]
            });
        }

        var userId = _currentUserService.GetRequiredUserId();
        var goal = Goal.Create(
            userId,
            request.Name.Trim(),
            request.Description,
            new Money(request.TargetAmount, request.Currency),
            new Money(request.InitialAmount, request.Currency));

        await _goalRepository.AddAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(goal);
    }

    public async Task<IReadOnlyList<GoalDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var goals = await _goalRepository.ListByUserAsync(userId, cancellationToken);
        return goals.Select(Map).ToList();
    }

    private static GoalDto Map(Goal goal)
    {
        return new GoalDto(
            goal.Id,
            goal.Name,
            goal.Description,
            goal.TargetAmount.Amount,
            goal.CurrentAmount.Amount,
            goal.TargetAmount.Currency,
            goal.ProgressPercentage);
    }
}
