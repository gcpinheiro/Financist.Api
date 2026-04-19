using Financist.Domain.Common;
using Financist.Domain.Enums;

namespace Financist.Domain.Entities;

public sealed class Category : AuditableEntity
{
    private Category()
    {
        Name = string.Empty;
    }

    private Category(Guid userId, string name, TransactionType type, bool isSystem)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Category user is required.");
        }

        UserId = userId;
        Rename(name);
        Type = type;
        IsSystem = isSystem;
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public TransactionType Type { get; private set; }

    public bool IsSystem { get; private set; }

    public User? User { get; private set; }

    public static Category Create(Guid userId, string name, TransactionType type, bool isSystem = false)
    {
        return new Category(userId, name, type, isSystem);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.");
        }

        Name = name.Trim();
        Touch();
    }
}
