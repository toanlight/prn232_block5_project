using System.Security.Claims;
using ClonEbay_CoreAPI.Models;

namespace ClonEbay_CoreAPI.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
