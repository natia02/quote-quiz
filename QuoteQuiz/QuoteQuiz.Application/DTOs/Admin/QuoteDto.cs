namespace QuoteQuiz.Application.DTOs.Admin;

public class QuoteDto
{
    public int Id { get; set; }
    public string QuoteText { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}