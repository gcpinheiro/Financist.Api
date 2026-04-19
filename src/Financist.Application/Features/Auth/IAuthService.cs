namespace Financist.Application.Features.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
