using QuoteQuiz.Domain.Common;

namespace QuoteQuiz.Domain.Entities;

public class Quote : BaseEntity
{
    public string QuoteText { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }

    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public ICollection<GameHistory> GameHistories { get; set; } = new List<GameHistory>();
    public ICollection<ShownQuote> ShownQuotes { get; set; } = new List<ShownQuote>();
}