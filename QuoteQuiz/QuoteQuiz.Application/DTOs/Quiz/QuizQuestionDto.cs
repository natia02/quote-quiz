namespace QuoteQuiz.Application.DTOs.Quiz;

public class QuizQuestionDto
{
    public int QuoteId { get; set; }
    public string QuoteText { get; set; } = string.Empty;
    public string DisplayedAuthor { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new(); // Empty for Binary mode, 3 items for Multiple Choice
    public string QuizMode { get; set; } = string.Empty; // "Binary" or "MultipleChoice"
}