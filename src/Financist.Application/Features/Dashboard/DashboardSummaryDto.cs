namespace Financist.Application.Features.Dashboard;

public sealed record DashboardSummaryDto(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance,
    int TransactionCount,
    int GoalCount,
    decimal AverageGoalProgress);
