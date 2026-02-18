using QuoteQuiz.Domain.Common;
using QuoteQuiz.Domain.Enums;

namespace QuoteQuiz.Domain.Entities;

public class GameHistory : BaseEntity
{
    public int UserId { get; set; }
    public int QuoteId { get; set; }
    public QuizMode QuizMode { get; set; }
    public string SelectedAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Quote Quote { get; set; } = null!;
}