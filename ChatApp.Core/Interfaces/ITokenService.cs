using ChatApp.Core.Entities;

namespace ChatApp.Core.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(ApplicationUser user);
}
