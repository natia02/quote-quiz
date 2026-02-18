using QuoteQuiz.Application.DTOs.Auth;

namespace QuoteQuiz.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<bool> UserExistsAsync(string email);
}