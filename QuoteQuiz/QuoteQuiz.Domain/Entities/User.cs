using QuoteQuiz.Domain.Common;
using QuoteQuiz.Domain.Enums;

namespace QuoteQuiz.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Quote> CreatedQuotes { get; set; } = new List<Quote>();
    public ICollection<GameHistory> GameHistories { get; set; } = new List<GameHistory>();
    public ICollection<ShownQuote> ShownQuotes { get; set; } = new List<ShownQuote>();
}