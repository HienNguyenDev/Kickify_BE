using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Authentication
{
    public interface IJwtProvider
    {
        Task<string> GetForCredentialsAsync(string email);
        string GenerateBackendJwt(User user);
        string GenerateRefreshToken();
    }
}
