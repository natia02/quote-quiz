using Microsoft.EntityFrameworkCore;
using QuoteQuiz.Domain.Entities;

namespace QuoteQuiz.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<GameHistory> GameHistories { get; set; }
    public DbSet<ShownQuote> ShownQuotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}