namespace QuoteQuiz.Application.DTOs.Admin;

public class UpdateQuoteDto
{
    public int Id { get; set; }
    public string QuoteText { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
}