using Financist.Domain.Enums;

namespace Financist.Application.Features.Categories;

public sealed record CategoryDto(Guid Id, string Name, TransactionType Type, bool IsSystem);
