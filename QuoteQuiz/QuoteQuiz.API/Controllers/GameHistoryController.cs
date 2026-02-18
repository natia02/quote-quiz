using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoteQuiz.API.Extensions;
using QuoteQuiz.Application.Interfaces.Services;

namespace QuoteQuiz.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GameHistoryController : ControllerBase
{
    private readonly IGameHistoryService _gameHistoryService;

    public GameHistoryController(IGameHistoryService gameHistoryService)
    {
        _gameHistoryService = gameHistoryService;
    }

    [HttpGet("my-history")]
    public async Task<IActionResult> GetMyHistory()
    {
        var history = await _gameHistoryService.GetUserHistoryAsync(User.GetUserId());
        return Ok(history);
    }

    [HttpGet("my-statistics")]
    public async Task<IActionResult> GetMyStatistics()
    {
        var stats = await _gameHistoryService.GetUserStatisticsAsync(User.GetUserId());
        return Ok(stats);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var history = await _gameHistoryService.GetAllHistoryAsync();
        return Ok(history);
    }

    [HttpGet("user/{userId}/statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserStatistics(int userId)
    {
        var stats = await _gameHistoryService.GetUserStatisticsAsync(userId);
        return Ok(stats);
    }
}