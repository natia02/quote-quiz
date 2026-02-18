using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteQuiz.Domain.Entities;

namespace QuoteQuiz.Infrastructure.Configurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.QuoteText)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(q => q.AuthorName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(q => q.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Index for filtering by author
        builder.HasIndex(q => q.AuthorName);
    }
}