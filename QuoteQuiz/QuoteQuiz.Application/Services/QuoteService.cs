using AutoMapper;
using QuoteQuiz.Application.DTOs.Admin;
using QuoteQuiz.Application.Interfaces.Repositories;
using QuoteQuiz.Application.Interfaces.Services;
using QuoteQuiz.Domain.Entities;
using QuoteQuiz.Domain.Exceptions;

namespace QuoteQuiz.Application.Services;

public class QuoteService : IQuoteService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public QuoteService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<QuoteDto>> GetAllQuotesAsync()
    {
        var quotes = await _unitOfWork.Quotes.GetAllAsync(q => q.CreatedBy);
        return quotes.Select(q =>
        {
            var dto = _mapper.Map<QuoteDto>(q);
            dto.CreatedByUserName = q.CreatedBy.Username;
            return dto;
        });
    }

    public async Task<QuoteDto?> GetQuoteByIdAsync(int id)
    {
        var quote = await _unitOfWork.Quotes.GetByIdAsync(id, q => q.CreatedBy);
        if (quote == null) return null;

        var dto = _mapper.Map<QuoteDto>(quote);
        dto.CreatedByUserName = quote.CreatedBy.Username;
        return dto;
    }

    public async Task<QuoteDto> CreateQuoteAsync(int createdByUserId, CreateQuoteDto createQuoteDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(createdByUserId);
        if (user == null)
            throw new NotFoundException("User not found");

        var quote = new Quote
        {
            QuoteText = createQuoteDto.QuoteText,
            AuthorName = createQuoteDto.AuthorName,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Quotes.AddAsync(quote);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<QuoteDto>(quote);
        dto.CreatedByUserName = user.Username;

        return dto;
    }

    public async Task<QuoteDto?> UpdateQuoteAsync(int id, UpdateQuoteDto updateQuoteDto)
    {
        var quote = await _unitOfWork.Quotes.GetByIdAsync(id, q => q.CreatedBy);
        if (quote == null) return null;

        quote.QuoteText = updateQuoteDto.QuoteText;
        quote.AuthorName = updateQuoteDto.AuthorName;

        _unitOfWork.Quotes.Update(quote);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<QuoteDto>(quote);
        dto.CreatedByUserName = quote.CreatedBy.Username;
        return dto;
    }

    public async Task<bool> DeleteQuoteAsync(int id)
    {
        var quote = await _unitOfWork.Quotes.GetByIdAsync(id);
        if (quote == null)
            return false;

        // Check if quote has been used in games
        var hasGameHistory = await _unitOfWork.GameHistories.AnyAsync(gh => gh.QuoteId == id);
        if (hasGameHistory)
            throw new ConflictException("Cannot delete quote that has been used in games");

        _unitOfWork.Quotes.Delete(quote);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<string>> GetAllAuthorsAsync()
    {
        var quotes = await _unitOfWork.Quotes.GetAllAsync();
        return quotes.Select(q => q.AuthorName).Distinct().OrderBy(a => a);
    }
}