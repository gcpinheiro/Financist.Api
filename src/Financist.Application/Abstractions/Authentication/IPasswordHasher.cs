namespace Financist.Application.Abstractions.Authentication;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string hashedPassword, string providedPassword);
}
