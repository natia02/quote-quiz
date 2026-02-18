using QuoteQuiz.Domain.Entities;

namespace QuoteQuiz.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}