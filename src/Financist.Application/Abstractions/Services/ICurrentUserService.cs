namespace Financist.Application.Abstractions.Services;

public interface ICurrentUserService
{
    Guid GetRequiredUserId();
}
