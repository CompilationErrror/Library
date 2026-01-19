using Microsoft.EntityFrameworkCore;
using LibraryApi.Data.DataSeeding.Helpers;

namespace LibraryApi.Data.DataSeeding
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDataAsync(LibraryContext context)
        {
            await SeedGenresAsync(context);

            await SeedBooksAsync(context);

            await SeedUsersAsync(context);

            // Seed ordered books (requires both books and users to exist)
            await SeedOrderedBooksAsync(context);
        }

        private static async Task SeedBooksAsync(LibraryContext context)
        {
            if (await context.Books.AnyAsync())
            {
                ConsoleBackgroundHelper.WriteGreen("Books already seeded, skipping...");
                return;
            }

            ConsoleBackgroundHelper.WriteGreen("Seeding books database...");

            var genres = await context.Genres.ToListAsync();

            var books = DataGenerator.GenerateSeedBooks(genres);

            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();

            ConsoleBackgroundHelper.WriteGreen($"Seeded {books.Count} books successfully!");
        }

        private static async Task SeedUsersAsync(LibraryContext context)
        {
            if (await context.Users.AnyAsync())
            {
                ConsoleBackgroundHelper.WriteGreen("Users already seeded, skipping...");
                return;
            }

            ConsoleBackgroundHelper.WriteGreen("Seeding users database...");

            var users = DataGenerator.GenerateSeedUsers();

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            ConsoleBackgroundHelper.WriteGreen($"Seeded {users.Count} users successfully!");
        }

        private static async Task SeedOrderedBooksAsync(LibraryContext context)
        {
            if (await context.OrderedBooks.AnyAsync())
            {
                ConsoleBackgroundHelper.WriteGreen("Ordered books already seeded, skipping...");
                return;
            }

            ConsoleBackgroundHelper.WriteGreen("Seeding ordered books database...");

            var orderedBooks = await DataGenerator.GenerateSeedOrderedBooksAsync(context);

            await context.OrderedBooks.AddRangeAsync(orderedBooks);
            await context.SaveChangesAsync();

            ConsoleBackgroundHelper.WriteGreen($"Seeded {orderedBooks.Count} ordered books successfully!");
        }

        private static async Task SeedGenresAsync(LibraryContext context)
        {
            if (await context.Genres.AnyAsync())
            {
                ConsoleBackgroundHelper.WriteGreen("Genres already seeded, skipping...");
                return;
            }

            ConsoleBackgroundHelper.WriteGreen("Seeding genres database...");

            var genres = DataGenerator.GenerateSeedGenres();

            await context.Genres.AddRangeAsync(genres);
            await context.SaveChangesAsync();

            ConsoleBackgroundHelper.WriteGreen($"Seeded {genres.Count} genres successfully!");
        }
    }
}