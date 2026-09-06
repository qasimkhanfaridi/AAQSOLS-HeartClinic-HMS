using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HeartClinicHms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace HeartClinicHms.Api.Services;

public static class AuthService
{
    public static bool VerifyPassword(User user, string password)
    {
        var hasher = new PasswordHasher<User>();
        return hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
    }

    public static string CreateToken(User user, IConfiguration config)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new("displayName", user.DisplayName),
            new(ClaimTypes.Role, user.Role?.Code ?? "USER"),
            new("branchId", user.BranchId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
