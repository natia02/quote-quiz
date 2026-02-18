namespace QuoteQuiz.Application.DTOs.Quiz;

public class AnswerResultDto
{
    public bool IsCorrect { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}