using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteQuiz.Domain.Entities;

namespace QuoteQuiz.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Unique constraints
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();

        // Relationships
        builder.HasMany(u => u.CreatedQuotes)
            .WithOne(q => q.CreatedBy)
            .HasForeignKey(q => q.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.GameHistories)
            .WithOne(gh => gh.User)
            .HasForeignKey(gh => gh.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ShownQuotes)
            .WithOne(sq => sq.User)
            .HasForeignKey(sq => sq.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}