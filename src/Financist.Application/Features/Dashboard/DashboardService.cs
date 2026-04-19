using Financist.Application.Abstractions.Persistence;
using Financist.Application.Abstractions.Services;
using Financist.Domain.Enums;

namespace Financist.Application.Features.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IGoalRepository _goalRepository;
    private readonly ITransactionRepository _transactionRepository;

    public DashboardService(
        ITransactionRepository transactionRepository,
        IGoalRepository goalRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _goalRepository = goalRepository;
        _currentUserService = currentUserService;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var transactions = await _transactionRepository.ListByUserAsync(userId, cancellationToken);
        var goals = await _goalRepository.ListByUserAsync(userId, cancellationToken);

        var totalIncome = transactions
            .Where(transaction => transaction.Type == TransactionType.Income)
            .Sum(transaction => transaction.Amount.Amount);

        var totalExpenses = transactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .Sum(transaction => transaction.Amount.Amount);

        var averageGoalProgress = goals.Count == 0
            ? 0m
            : decimal.Round(goals.Average(goal => goal.ProgressPercentage), 2, MidpointRounding.AwayFromZero);

        return new DashboardSummaryDto(
            totalIncome,
            totalExpenses,
            totalIncome - totalExpenses,
            transactions.Count,
            goals.Count,
            averageGoalProgress);
    }
}
