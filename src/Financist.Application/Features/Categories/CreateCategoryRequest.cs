using Financist.Domain.Enums;

namespace Financist.Application.Features.Categories;

public sealed record CreateCategoryRequest(string Name, TransactionType Type);
