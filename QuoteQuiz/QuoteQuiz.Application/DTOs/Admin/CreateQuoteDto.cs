namespace QuoteQuiz.Application.DTOs.Admin;

public class CreateQuoteDto
{
    public string QuoteText { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
}