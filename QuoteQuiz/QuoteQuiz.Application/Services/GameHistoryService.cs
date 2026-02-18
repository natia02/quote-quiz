using AutoMapper;
using QuoteQuiz.Application.DTOs.Admin;
using QuoteQuiz.Application.Interfaces.Repositories;
using QuoteQuiz.Application.Interfaces.Services;
using QuoteQuiz.Domain.Enums;

namespace QuoteQuiz.Application.Services;

public class GameHistoryService : IGameHistoryService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GameHistoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GameHistoryDto>> GetUserHistoryAsync(int userId)
    {
        var history = await _unitOfWork.GameHistories.FindAsync(
            gh => gh.UserId == userId,
            gh => gh.User,
            gh => gh.Quote);

        return history
            .OrderByDescending(gh => gh.AnsweredAt)
            .Select(gh => _mapper.Map<GameHistoryDto>(gh));
    }

    public async Task<IEnumerable<GameHistoryDto>> GetAllHistoryAsync()
    {
        var history = await _unitOfWork.GameHistories.GetAllAsync(
            gh => gh.User,
            gh => gh.Quote);

        return history
            .OrderByDescending(gh => gh.AnsweredAt)
            .Select(gh => _mapper.Map<GameHistoryDto>(gh));
    }

    public async Task<Dictionary<string, object>> GetUserStatisticsAsync(int userId)
    {
        var history = (await _unitOfWork.GameHistories.FindAsync(gh => gh.UserId == userId)).ToList();

        var totalGames = history.Count;
        var correctAnswers = history.Count(gh => gh.IsCorrect);
        var successRate = totalGames > 0 ? (double)correctAnswers / totalGames * 100 : 0;

        return new Dictionary<string, object>
        {
            { "totalGames", totalGames },
            { "correctAnswers", correctAnswers },
            { "wrongAnswers", totalGames - correctAnswers },
            { "successRate", Math.Round(successRate, 2) },
            { "binaryGames", history.Count(gh => gh.QuizMode == QuizMode.Binary) },
            { "multipleChoiceGames", history.Count(gh => gh.QuizMode == QuizMode.MultipleChoice) }
        };
    }
}