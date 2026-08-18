using System.Security.Claims;

namespace MarketPlace.Api.Common.Extensions;

public static class ClaimPrincipalExtention
{
    extension(ClaimsPrincipal user)
    {
        
        public Guid UserId => Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : Guid.Empty;

        public IReadOnlyCollection<string> UserRoles => [..user.FindAll(ClaimTypes.Role).Select(c => c.Value)];
        
        public (Guid userId, IReadOnlyCollection<string> roles) UserIdAndRole => (user.UserId, user.UserRoles);
              

    }
}
