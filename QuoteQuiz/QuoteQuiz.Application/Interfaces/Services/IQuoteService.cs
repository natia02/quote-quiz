using QuoteQuiz.Application.DTOs.Admin;

namespace QuoteQuiz.Application.Interfaces.Services;

public interface IQuoteService
{
    Task<IEnumerable<QuoteDto>> GetAllQuotesAsync();
    Task<QuoteDto?> GetQuoteByIdAsync(int id);
    Task<QuoteDto> CreateQuoteAsync(int createdByUserId, CreateQuoteDto createQuoteDto);
    Task<QuoteDto?> UpdateQuoteAsync(int id, UpdateQuoteDto updateQuoteDto);
    Task<bool> DeleteQuoteAsync(int id);
    Task<IEnumerable<string>> GetAllAuthorsAsync();
}