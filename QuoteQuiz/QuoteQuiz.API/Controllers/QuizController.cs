using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoteQuiz.API.Extensions;
using QuoteQuiz.Application.DTOs.Quiz;
using QuoteQuiz.Application.Interfaces.Services;
using QuoteQuiz.Domain.Enums;

namespace QuoteQuiz.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    
    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }
    
    [HttpGet("question")]
    public async Task<IActionResult> GetNextQuestion([FromQuery] string mode = "Binary")
    {
        if (!Enum.TryParse<QuizMode>(mode, out var quizMode))
            quizMode = QuizMode.Binary;
        
        var question = await _quizService.GetNextQuestionAsync(User.GetUserId(), quizMode);
        return Ok(question);
    }
    
    [HttpPost("answer")]
    public async Task<IActionResult> SubmitAnswer(SubmitAnswerDto submitAnswerDto)
    {
        var result = await _quizService.SubmitAnswerAsync(User.GetUserId(), submitAnswerDto);
        return Ok(result);
    }
}