using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteQuiz.Domain.Entities;

namespace QuoteQuiz.Infrastructure.Configurations;

public class ShownQuoteConfiguration : IEntityTypeConfiguration<ShownQuote>
{
    public void Configure(EntityTypeBuilder<ShownQuote> builder)
    {
        builder.HasKey(sq => sq.Id);

        builder.Property(sq => sq.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Composite unique index - same quote can't be shown twice to same user
        builder.HasIndex(sq => new { sq.UserId, sq.QuoteId }).IsUnique();
    }
}