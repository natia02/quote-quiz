using QuoteQuiz.Application.DTOs.Quiz;
using QuoteQuiz.Domain.Enums;

namespace QuoteQuiz.Application.Interfaces.Services;

public interface IQuizService
{
    Task<QuizQuestionDto?> GetNextQuestionAsync(int userId, QuizMode quizMode);
    Task<AnswerResultDto> SubmitAnswerAsync(int userId, SubmitAnswerDto submitAnswerDto);
}