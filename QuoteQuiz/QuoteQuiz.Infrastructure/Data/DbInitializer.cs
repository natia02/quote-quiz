using QuoteQuiz.Domain.Entities;
using QuoteQuiz.Domain.Enums;

namespace QuoteQuiz.Infrastructure.Data;

public class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Users.Any())
            return;

        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@flatrock.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        var quotes = new List<Quote>
        {
            new()
            {
                QuoteText =
                    "A dreamer is one who can only find his way by moonlight, and his punishment is that he sees the dawn before the rest of the world.",
                AuthorName = "Oscar Wilde",
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                QuoteText =
                    "It has been said that democracy is the worst form of government except all the others that have been tried.",
                AuthorName = "Winston Churchill",
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                QuoteText = "The only thing we have to fear is fear itself.",
                AuthorName = "Franklin D. Roosevelt",
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                QuoteText =
                    "To be yourself in a world that is constantly trying to make you something else is the greatest accomplishment.",
                AuthorName = "Ralph Waldo Emerson",
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                QuoteText = "In three words I can sum up everything I've learned about life: it goes on.",
                AuthorName = "Robert Frost",
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                QuoteText = "The only way to do great work is to love what you do.",
                AuthorName = "Steve Jobs",
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                QuoteText = "Life is what happens when you're busy making other plans.",
                AuthorName = "John Lennon",
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                QuoteText = "The future belongs to those who believe in the beauty of their dreams.",
                AuthorName = "Eleanor Roosevelt",
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                QuoteText = "It is during our darkest moments that we must focus to see the light.",
                AuthorName = "Aristotle",
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                QuoteText = "The only impossible journey is the one you never begin.",
                AuthorName = "Tony Robbins",
                CreatedByUserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Quotes.AddRange(quotes);
        await context.SaveChangesAsync();
    }
}