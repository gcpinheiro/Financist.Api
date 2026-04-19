using System.Security.Claims;
using Financist.Application.Abstractions.Services;
using Financist.Application.Common.Exceptions;

namespace Financist.Api.Common;

public sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetRequiredUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? httpContext?.User.FindFirstValue("sub");

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            throw new UnauthorizedException("Authentication is required to access this resource.");
        }

        return parsedUserId;
    }
}
