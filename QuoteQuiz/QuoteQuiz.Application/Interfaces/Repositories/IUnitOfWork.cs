using QuoteQuiz.Domain.Entities;

namespace QuoteQuiz.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Quote> Quotes { get; }
    IRepository<GameHistory> GameHistories { get; }
    IRepository<ShownQuote> ShownQuotes { get; }

    Task<int> SaveChangesAsync();
}