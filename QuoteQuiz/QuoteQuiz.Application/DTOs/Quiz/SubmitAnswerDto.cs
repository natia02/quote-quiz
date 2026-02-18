namespace QuoteQuiz.Application.DTOs.Quiz;

public class SubmitAnswerDto
{
    public int QuoteId { get; set; }
    public string SelectedAnswer { get; set; } = string.Empty;
    public string QuizMode { get; set; } = string.Empty;
    public string DisplayedAuthor { get; set; } = string.Empty;
}