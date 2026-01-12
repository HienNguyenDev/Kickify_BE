namespace Kickify.Application.Abstractions.Authentication
{
    public interface IAuthenticationServices
    {
        Task<string> RegisterAsync(string email, string password);
        Task DeleteUserAsync(string identityId);
    }
}
