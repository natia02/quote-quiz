using QuoteQuiz.Application.DTOs.Admin;

namespace QuoteQuiz.Application.Interfaces.Services;

public interface IGameHistoryService
{
    Task<IEnumerable<GameHistoryDto>> GetUserHistoryAsync(int userId);
    Task<IEnumerable<GameHistoryDto>> GetAllHistoryAsync();
    Task<Dictionary<string, object>> GetUserStatisticsAsync(int userId);
}