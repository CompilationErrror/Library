using System.Security.Claims;

namespace LibraryApi.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var val = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(val, out var id) ? id : Guid.Empty;
        }
    }
}
