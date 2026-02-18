using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteQuiz.Domain.Entities;

namespace QuoteQuiz.Infrastructure.Configurations;

public class GameHistoryConfiguration : IEntityTypeConfiguration<GameHistory>
{
    public void Configure(EntityTypeBuilder<GameHistory> builder)
    {
        builder.HasKey(gh => gh.Id);

        builder.Property(gh => gh.QuizMode)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(gh => gh.SelectedAnswer)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(gh => gh.IsCorrect)
            .IsRequired();

        builder.Property(gh => gh.AnsweredAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes for filtering/sorting
        builder.HasIndex(gh => gh.UserId);
        builder.HasIndex(gh => gh.AnsweredAt);
        builder.HasIndex(gh => new { gh.UserId, gh.AnsweredAt });
    }
}