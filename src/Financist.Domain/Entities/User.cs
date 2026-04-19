using Financist.Domain.Common;

namespace Financist.Domain.Entities;

public sealed class User : AuditableEntity
{
    private User()
    {
        FullName = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    private User(string fullName, string email, string passwordHash)
    {
        SetFullName(fullName);
        SetEmail(email);
        SetPasswordHash(passwordHash);
        IsActive = true;
    }

    public string FullName { get; private set; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public bool IsActive { get; private set; }

    public static User Create(string fullName, string email, string passwordHash)
    {
        return new User(fullName, email, passwordHash);
    }

    public void ChangeName(string fullName)
    {
        SetFullName(fullName);
        Touch();
    }

    public void ChangePassword(string passwordHash)
    {
        SetPasswordHash(passwordHash);
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    private void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("User full name is required.");
        }

        FullName = fullName.Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("User email is required.");
        }

        Email = email.Trim().ToLowerInvariant();
    }

    private void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        PasswordHash = passwordHash;
    }
}
