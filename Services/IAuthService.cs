namespace Application.Services;

public interface IAuthService
{
    string GenerateToken(int userId, string email);
}

