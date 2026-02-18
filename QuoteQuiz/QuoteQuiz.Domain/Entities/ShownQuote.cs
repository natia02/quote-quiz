using QuoteQuiz.Domain.Common;

namespace QuoteQuiz.Domain.Entities;

public class ShownQuote : BaseEntity
{
    public int UserId { get; set; }
    public int QuoteId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Quote Quote { get; set; } = null!;
}