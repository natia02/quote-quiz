using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoteQuiz.API.Extensions;
using QuoteQuiz.Application.DTOs.Admin;
using QuoteQuiz.Application.Interfaces.Services;

namespace QuoteQuiz.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;

    public QuotesController(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var quotes = await _quoteService.GetAllQuotesAsync();
        return Ok(quotes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var quote = await _quoteService.GetQuoteByIdAsync(id);
        if (quote == null)
            return NotFound();

        return Ok(quote);
    }

    [HttpGet("authors")]
    public async Task<IActionResult> GetAuthors()
    {
        var authors = await _quoteService.GetAllAuthorsAsync();
        return Ok(authors);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateQuoteDto createQuoteDto)
    {
        var quote = await _quoteService.CreateQuoteAsync(User.GetUserId(), createQuoteDto);
        return CreatedAtAction(nameof(GetById), new { id = quote.Id }, quote);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateQuoteDto updateQuoteDto)
    {
        var quote = await _quoteService.UpdateQuoteAsync(id, updateQuoteDto);
        if (quote == null)
            return NotFound();

        return Ok(quote);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _quoteService.DeleteQuoteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
    
}