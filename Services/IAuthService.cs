using Matoli.Api.Models;

namespace Matoli.Api.Services;

public interface IAuthService
{
    bool ValidateCredentials(string username, string password);
    LoginResponse GenerateToken(string username);
}
