using BookingSystem.Api.Extensions;
using BookingSystem.Application.Common.Abstractions;

namespace BookingSystem.Api.Services;

public class CurrentUserService (IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId => httpContextAccessor.HttpContext?.User.GetUserIdOrDefault();

    public Guid GetRequiredUserId()
        => httpContextAccessor.HttpContext!.User.GetUserId();
}