using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using QuoteQuiz.Application.DTOs.Quiz;
using QuoteQuiz.Application.Interfaces.Repositories;
using QuoteQuiz.Application.Services;
using QuoteQuiz.Domain.Entities;
using QuoteQuiz.Domain.Enums;
using QuoteQuiz.Domain.Exceptions;
using Xunit;

namespace QuoteQuiz.Tests.Services;

public class QuizServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<Quote>> _mockQuotesRepo;
    private readonly Mock<IRepository<ShownQuote>> _mockShownQuotesRepo;
    private readonly Mock<IRepository<GameHistory>> _mockGameHistoriesRepo;
    private readonly QuizService _quizService;

    public QuizServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockQuotesRepo = new Mock<IRepository<Quote>>();
        _mockShownQuotesRepo = new Mock<IRepository<ShownQuote>>();
        _mockGameHistoriesRepo = new Mock<IRepository<GameHistory>>();

        _mockUnitOfWork.Setup(u => u.Quotes).Returns(_mockQuotesRepo.Object);
        _mockUnitOfWork.Setup(u => u.ShownQuotes).Returns(_mockShownQuotesRepo.Object);
        _mockUnitOfWork.Setup(u => u.GameHistories).Returns(_mockGameHistoriesRepo.Object);

        _quizService = new QuizService(_mockUnitOfWork.Object);
    }

    // Creates quotes where every quote has a unique author
    private static List<Quote> MakeQuotes(int count) =>
        Enumerable.Range(1, count)
            .Select(i => new Quote
            {
                Id = i,
                QuoteText = $"Quote text {i}",
                AuthorName = $"Author {i}",
                CreatedByUserId = 1
            })
            .ToList();

    private void SetupNoShownQuotes(int userId = 1) =>
        _mockShownQuotesRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ShownQuote, bool>>>()))
            .ReturnsAsync(new List<ShownQuote>());

    #region GetNextQuestionAsync — Binary mode

    [Fact]
    public async Task GetNextQuestionAsync_BinaryMode_ReturnsBinaryQuestion()
    {
        // Arrange
        SetupNoShownQuotes();
        _mockQuotesRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(MakeQuotes(5));

        // Act
        var result = await _quizService.GetNextQuestionAsync(userId: 1, QuizMode.Binary);

        // Assert
        result.Should().NotBeNull();
        result!.QuizMode.Should().Be("Binary");
        result.Options.Should().BeEmpty();           // Binary has no options list
        result.DisplayedAuthor.Should().NotBeEmpty(); // Binary shows a single author name
    }

    [Fact]
    public async Task GetNextQuestionAsync_BinaryMode_DisplayedAuthorIsOneOfTheAuthorsInSystem()
    {
        // Arrange
        var quotes = MakeQuotes(5);
        SetupNoShownQuotes();
        _mockQuotesRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(quotes);

        // Act
        var result = await _quizService.GetNextQuestionAsync(userId: 1, QuizMode.Binary);

        // Assert — displayed author must come from the actual author pool
        var allAuthors = quotes.Select(q => q.AuthorName).ToList();
        allAuthors.Should().Contain(result!.DisplayedAuthor);
    }

    #endregion

    #region GetNextQuestionAsync — Multiple choice mode

    [Fact]
    public async Task GetNextQuestionAsync_MultipleChoiceMode_ReturnsThreeOptions()
    {
        // Arrange
        SetupNoShownQuotes();
        _mockQuotesRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(MakeQuotes(5));

        // Act
        var result = await _quizService.GetNextQuestionAsync(userId: 1, QuizMode.MultipleChoice);

        // Assert
        result.Should().NotBeNull();
        result!.QuizMode.Should().Be("MultipleChoice");
        result.Options.Should().HaveCount(3);
        result.DisplayedAuthor.Should().BeEmpty(); // Multiple choice shows options, not a displayed author
    }

    [Fact]
    public async Task GetNextQuestionAsync_MultipleChoiceMode_OptionsAlwaysContainCorrectAuthor()
    {
        // Arrange
        var quotes = MakeQuotes(5);
        SetupNoShownQuotes();
        _mockQuotesRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(quotes);

        // Act
        var result = await _quizService.GetNextQuestionAsync(userId: 1, QuizMode.MultipleChoice);

        // Assert — the correct answer must always be one of the options
        var correctQuote = quotes.First(q => q.Id == result!.QuoteId);
        result!.Options.Should().Contain(correctQuote.AuthorName);
    }

    [Fact]
    public async Task GetNextQuestionAsync_MultipleChoiceMode_OptionsAreUnique()
    {
        // Arrange
        SetupNoShownQuotes();
        _mockQuotesRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(MakeQuotes(5));

        // Act
        var result = await _quizService.GetNextQuestionAsync(userId: 1, QuizMode.MultipleChoice);

        // Assert — no duplicate options
        result!.Options.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GetNextQuestionAsync_NotEnoughAuthors_ThrowsValidationException()
    {
        // Arrange — only 2 quotes (2 unique authors); need at least 3 for multiple choice
        var quotes = new List<Quote>
        {
            new() { Id = 1, QuoteText = "Quote 1", AuthorName = "Author A", CreatedByUserId = 1 },
            new() { Id = 2, QuoteText = "Quote 2", AuthorName = "Author B", CreatedByUserId = 1 }
        };

        SetupNoShownQuotes();
        _mockQuotesRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(quotes);

        // Act
        var act = async () => await _quizService.GetNextQuestionAsync(userId: 1, QuizMode.MultipleChoice);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*multiple choice*");
    }

    #endregion

    #region GetNextQuestionAsync — Progress tracking

    [Fact]
    public async Task GetNextQuestionAsync_UnseenQuotesExist_OnlyReturnsUnseenQuote()
    {
        // Arrange — quote 1 already seen, only quote 2 and 3 are unseen
        var quotes = MakeQuotes(3);
        var shownQuotes = new List<ShownQuote> { new() { UserId = 1, QuoteId = 1 } };

        _mockShownQuotesRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ShownQuote, bool>>>()))
            .ReturnsAsync(shownQuotes);

        _mockQuotesRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(quotes);

        // Act
        var result = await _quizService.GetNextQuestionAsync(userId: 1, QuizMode.Binary);

        // Assert — returned quote must NOT be the already-seen quote 1
        result!.QuoteId.Should().NotBe(1);
    }

    [Fact]
    public async Task GetNextQuestionAsync_AllQuotesSeen_ResetsProgressAndReturnsQuestion()
    {
        // Arrange — all 3 quotes already seen
        var quotes = MakeQuotes(3);
        var shownQuotes = quotes.Select(q => new ShownQuote { UserId = 1, QuoteId = q.Id }).ToList();

        _mockShownQuotesRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ShownQuote, bool>>>()))
            .ReturnsAsync(shownQuotes);

        _mockQuotesRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(quotes);

        // Act
        var result = await _quizService.GetNextQuestionAsync(userId: 1, QuizMode.Binary);

        // Assert — shown quotes were deleted (progress reset)
        _mockShownQuotesRepo.Verify(r => r.Delete(It.IsAny<ShownQuote>()), Times.Exactly(3));
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNextQuestionAsync_NoQuotesInSystem_ThrowsNotFoundException()
    {
        // Arrange
        SetupNoShownQuotes();
        _mockQuotesRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Quote>());

        // Act
        var act = async () => await _quizService.GetNextQuestionAsync(userId: 1, QuizMode.Binary);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*quotes*");
    }

    #endregion

    #region SubmitAnswerAsync

    [Fact]
    public async Task SubmitAnswerAsync_CorrectAnswer_ReturnsIsCorrectTrue()
    {
        // Arrange
        var quote = new Quote { Id = 1, QuoteText = "Quote 1", AuthorName = "Albert Einstein", CreatedByUserId = 1 };
        var dto = new SubmitAnswerDto { QuoteId = 1, SelectedAnswer = "Albert Einstein", QuizMode = "Binary" };

        _mockQuotesRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quote);
        _mockShownQuotesRepo
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<ShownQuote, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _quizService.SubmitAnswerAsync(userId: 1, dto);

        // Assert
        result.IsCorrect.Should().BeTrue();
        result.CorrectAnswer.Should().Be("Albert Einstein");
        result.Message.Should().Contain("Correct");
    }

    [Fact]
    public async Task SubmitAnswerAsync_WrongAnswer_ReturnsIsCorrectFalse()
    {
        // Arrange
        var quote = new Quote { Id = 1, QuoteText = "Quote 1", AuthorName = "Albert Einstein", CreatedByUserId = 1 };
        var dto = new SubmitAnswerDto { QuoteId = 1, SelectedAnswer = "Wrong Author", QuizMode = "Binary" };

        _mockQuotesRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quote);
        _mockShownQuotesRepo
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<ShownQuote, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _quizService.SubmitAnswerAsync(userId: 1, dto);

        // Assert
        result.IsCorrect.Should().BeFalse();
        result.CorrectAnswer.Should().Be("Albert Einstein");
        result.Message.Should().Contain("wrong");
    }

    [Fact]
    public async Task SubmitAnswerAsync_CorrectAnswer_IsCaseInsensitive()
    {
        // Arrange
        var quote = new Quote { Id = 1, QuoteText = "Quote 1", AuthorName = "Albert Einstein", CreatedByUserId = 1 };
        var dto = new SubmitAnswerDto { QuoteId = 1, SelectedAnswer = "albert einstein", QuizMode = "Binary" };

        _mockQuotesRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quote);
        _mockShownQuotesRepo
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<ShownQuote, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var result = await _quizService.SubmitAnswerAsync(userId: 1, dto);

        // Assert — answer comparison is case-insensitive
        result.IsCorrect.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitAnswerAsync_QuoteNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var dto = new SubmitAnswerDto { QuoteId = 999, SelectedAnswer = "Someone", QuizMode = "Binary" };

        _mockQuotesRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Quote?)null);

        // Act
        var act = async () => await _quizService.SubmitAnswerAsync(userId: 1, dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Quote*");
    }

    [Fact]
    public async Task SubmitAnswerAsync_AlwaysSavesGameHistory()
    {
        // Arrange
        var quote = new Quote { Id = 1, QuoteText = "Quote 1", AuthorName = "Einstein", CreatedByUserId = 1 };
        var dto = new SubmitAnswerDto { QuoteId = 1, SelectedAnswer = "Einstein", QuizMode = "Binary" };

        _mockQuotesRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quote);
        _mockShownQuotesRepo
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<ShownQuote, bool>>>()))
            .ReturnsAsync(false);

        // Act
        await _quizService.SubmitAnswerAsync(userId: 1, dto);

        // Assert — game history is always recorded
        _mockGameHistoriesRepo.Verify(r => r.AddAsync(It.IsAny<GameHistory>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswerAsync_FirstTimeSeeingQuote_AddsShownQuote()
    {
        // Arrange
        var quote = new Quote { Id = 1, QuoteText = "Quote 1", AuthorName = "Einstein", CreatedByUserId = 1 };
        var dto = new SubmitAnswerDto { QuoteId = 1, SelectedAnswer = "Einstein", QuizMode = "Binary" };

        _mockQuotesRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quote);
        _mockShownQuotesRepo
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<ShownQuote, bool>>>()))
            .ReturnsAsync(false); // not yet marked as shown

        // Act
        await _quizService.SubmitAnswerAsync(userId: 1, dto);

        // Assert
        _mockShownQuotesRepo.Verify(r => r.AddAsync(It.IsAny<ShownQuote>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswerAsync_QuoteAlreadyShown_DoesNotAddDuplicateShownQuote()
    {
        // Arrange
        var quote = new Quote { Id = 1, QuoteText = "Quote 1", AuthorName = "Einstein", CreatedByUserId = 1 };
        var dto = new SubmitAnswerDto { QuoteId = 1, SelectedAnswer = "Einstein", QuizMode = "Binary" };

        _mockQuotesRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quote);
        _mockShownQuotesRepo
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<ShownQuote, bool>>>()))
            .ReturnsAsync(true); // already shown

        // Act
        await _quizService.SubmitAnswerAsync(userId: 1, dto);

        // Assert — no duplicate insert
        _mockShownQuotesRepo.Verify(r => r.AddAsync(It.IsAny<ShownQuote>()), Times.Never);
    }

    [Fact]
    public async Task SubmitAnswerAsync_RecordsCorrectGameHistoryFields()
    {
        // Arrange
        var quote = new Quote { Id = 5, QuoteText = "Quote", AuthorName = "Einstein", CreatedByUserId = 1 };
        var dto = new SubmitAnswerDto { QuoteId = 5, SelectedAnswer = "Einstein", QuizMode = "MultipleChoice" };

        _mockQuotesRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(quote);
        _mockShownQuotesRepo
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<ShownQuote, bool>>>()))
            .ReturnsAsync(false);

        GameHistory? captured = null;
        _mockGameHistoriesRepo
            .Setup(r => r.AddAsync(It.IsAny<GameHistory>()))
            .Callback<GameHistory>(gh => captured = gh);

        // Act
        await _quizService.SubmitAnswerAsync(userId: 7, dto);

        // Assert — all fields are set correctly
        captured.Should().NotBeNull();
        captured!.UserId.Should().Be(7);
        captured.QuoteId.Should().Be(5);
        captured.QuizMode.Should().Be(QuizMode.MultipleChoice);
        captured.SelectedAnswer.Should().Be("Einstein");
        captured.IsCorrect.Should().BeTrue();
    }

    #endregion
}
