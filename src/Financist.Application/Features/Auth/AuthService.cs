using Financist.Application.Abstractions.Authentication;
using Financist.Application.Abstractions.Persistence;
using Financist.Application.Common.Exceptions;

namespace Financist.Application.Features.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _userRepository;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("The user account is inactive.");
        }

        var token = _jwtTokenGenerator.Generate(user);
        return new LoginResponse(token.AccessToken, token.ExpiresAtUtc, user.FullName, user.Email);
    }

    private static void Validate(LoginRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors["password"] = ["Password is required."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}
