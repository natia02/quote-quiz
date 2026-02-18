using QuoteQuiz.Application.DTOs.Quiz;
using QuoteQuiz.Application.Interfaces.Repositories;
using QuoteQuiz.Application.Interfaces.Services;
using QuoteQuiz.Domain.Entities;
using QuoteQuiz.Domain.Enums;
using QuoteQuiz.Domain.Exceptions;

namespace QuoteQuiz.Application.Services;

public class QuizService : IQuizService
{
    private readonly IUnitOfWork _unitOfWork;

    public QuizService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<QuizQuestionDto?> GetNextQuestionAsync(int userId, QuizMode quizMode)
    {
        var shownQuoteIds = (await _unitOfWork.ShownQuotes
                .FindAsync(sq => sq.UserId == userId))
            .Select(sq => sq.QuoteId)
            .ToList();

        var availableQuotes = await _unitOfWork.Quotes.GetAllAsync();
        var unseenQuotes = availableQuotes.Where(q => !shownQuoteIds.Contains(q.Id)).ToList();

        if (!unseenQuotes.Any())
        {
            // User has seen all quotes - reset their progress
            var userShownQuotes = await _unitOfWork.ShownQuotes.FindAsync(sq => sq.UserId == userId);
            foreach (var shownQuote in userShownQuotes) _unitOfWork.ShownQuotes.Delete(shownQuote);
            await _unitOfWork.SaveChangesAsync();

            unseenQuotes = availableQuotes.ToList();
        }

        if (!unseenQuotes.Any())
            throw new NotFoundException("No quotes available in the system");

        var random = Random.Shared;
        var selectedQuote = unseenQuotes[random.Next(unseenQuotes.Count)];

        return quizMode == QuizMode.Binary
            ? FormatBinaryQuestion(selectedQuote, availableQuotes, random)
            : FormatMultipleChoiceQuestion(selectedQuote, availableQuotes, random);
    }

    public async Task<AnswerResultDto> SubmitAnswerAsync(int userId, SubmitAnswerDto submitAnswerDto)
    {
        var quote = await _unitOfWork.Quotes.GetByIdAsync(submitAnswerDto.QuoteId);
        if (quote == null)
            throw new NotFoundException("Quote not found");

        var parsedMode = Enum.Parse<QuizMode>(submitAnswerDto.QuizMode);

        bool isCorrect;
        if (parsedMode == QuizMode.Binary)
        {
            var userAgreed = submitAnswerDto.SelectedAnswer.Equals(
                submitAnswerDto.DisplayedAuthor, StringComparison.OrdinalIgnoreCase);
            var displayedWasCorrect = submitAnswerDto.DisplayedAuthor.Equals(
                quote.AuthorName, StringComparison.OrdinalIgnoreCase);
            isCorrect = userAgreed == displayedWasCorrect;
        }
        else
        {
            isCorrect = submitAnswerDto.SelectedAnswer.Equals(
                quote.AuthorName, StringComparison.OrdinalIgnoreCase);
        }

        var gameHistory = new GameHistory
        {
            UserId = userId,
            QuoteId = submitAnswerDto.QuoteId,
            QuizMode = parsedMode,
            SelectedAnswer = submitAnswerDto.SelectedAnswer,
            IsCorrect = isCorrect,
            AnsweredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.GameHistories.AddAsync(gameHistory);

        var alreadyShown = await _unitOfWork.ShownQuotes.AnyAsync(
            sq => sq.UserId == userId && sq.QuoteId == submitAnswerDto.QuoteId);

        if (!alreadyShown)
        {
            var shownQuote = new ShownQuote
            {
                UserId = userId,
                QuoteId = submitAnswerDto.QuoteId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.ShownQuotes.AddAsync(shownQuote);
        }

        await _unitOfWork.SaveChangesAsync();

        return new AnswerResultDto
        {
            IsCorrect = isCorrect,
            CorrectAnswer = quote.AuthorName,
            Message = isCorrect
                ? $"Correct! The right answer is: {quote.AuthorName}"
                : $"Sorry, you are wrong! The right answer is: {quote.AuthorName}"
        };
    }

    private static QuizQuestionDto FormatBinaryQuestion(Quote quote, IEnumerable<Quote> allQuotes, Random random)
    {
        var otherAuthors = allQuotes
            .Where(q => q.Id != quote.Id && q.AuthorName != quote.AuthorName)
            .Select(q => q.AuthorName)
            .Distinct()
            .ToList();

        // 50/50: show correct author or a random wrong one
        var showCorrectAuthor = !otherAuthors.Any() || random.Next(2) == 0;

        var displayedAuthor = showCorrectAuthor
            ? quote.AuthorName
            : otherAuthors[random.Next(otherAuthors.Count)];

        return new QuizQuestionDto
        {
            QuoteId = quote.Id,
            QuoteText = quote.QuoteText,
            DisplayedAuthor = displayedAuthor,
            Options = new List<string>(),
            QuizMode = "Binary"
        };
    }

    private static QuizQuestionDto FormatMultipleChoiceQuestion(Quote quote, IEnumerable<Quote> allQuotes, Random random)
    {
        var otherAuthors = allQuotes
            .Where(q => q.AuthorName != quote.AuthorName)
            .Select(q => q.AuthorName)
            .Distinct()
            .ToList();

        if (otherAuthors.Count < 2)
            throw new ValidationException(
                "Not enough authors in the system for multiple choice mode (minimum 3 required)");

        var wrongAuthors = otherAuthors.OrderBy(_ => random.Next()).Take(2).ToList();

        var options = new List<string> { quote.AuthorName };
        options.AddRange(wrongAuthors);
        options = options.OrderBy(_ => random.Next()).ToList();

        return new QuizQuestionDto
        {
            QuoteId = quote.Id,
            QuoteText = quote.QuoteText,
            DisplayedAuthor = string.Empty,
            Options = options,
            QuizMode = "MultipleChoice"
        };
    }
}