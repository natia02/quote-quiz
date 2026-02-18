namespace QuoteQuiz.Application.DTOs.Admin;

public class GameHistoryDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string QuoteText { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string QuizMode { get; set; } = string.Empty;
    public string SelectedAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; }
}