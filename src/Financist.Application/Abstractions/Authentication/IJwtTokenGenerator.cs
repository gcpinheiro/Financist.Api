using Financist.Domain.Entities;

namespace Financist.Application.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    GeneratedToken Generate(User user);
}
