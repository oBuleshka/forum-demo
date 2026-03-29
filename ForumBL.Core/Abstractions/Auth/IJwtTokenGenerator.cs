using Forum.Domain.Entities;

namespace ForumBL.Core.Abstractions.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
