using QuoteQuiz.Application.Interfaces.Repositories;
using QuoteQuiz.Domain.Entities;
using QuoteQuiz.Infrastructure.Data;

namespace QuoteQuiz.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        Users = new Repository<User>(_context);
        Quotes = new Repository<Quote>(_context);
        GameHistories = new Repository<GameHistory>(_context);
        ShownQuotes = new Repository<ShownQuote>(_context);
    }

    public IRepository<User> Users { get; }
    public IRepository<Quote> Quotes { get; }
    public IRepository<GameHistory> GameHistories { get; }
    public IRepository<ShownQuote> ShownQuotes { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}