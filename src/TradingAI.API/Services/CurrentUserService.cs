using System.Security.Claims;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.API.Services
{
    public class CurrentUserService(IHttpContextAccessor accessor):ICurrentUserService
    {
        public Guid? UserId
        {
            get
            {
                var raw = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? accessor.HttpContext?.User.FindFirstValue("sub");

                return Guid.TryParse(raw, out var id) ? id : null;
            }
        }

        public bool IsAuthenticated => accessor.HttpContext?.User.Identity?.IsAuthenticated == true;

        public Guid RequireUserId() => UserId ?? throw new UnauthorizedAccessException("User is not authenticated");

    }
}
