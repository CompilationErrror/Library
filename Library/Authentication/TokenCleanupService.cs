using LibraryApi.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Authentication
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval;

        private readonly DateTime _maxExpDate = DateTime.Now.AddDays(-14);

        public TokenCleanupService(
            IServiceProvider serviceProvider,
            ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cleanupInterval = TimeSpan.FromHours(24); 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupTokensAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up revoked tokens");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }

        private async Task CleanupTokensAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();

            var tokensToDelete = await context.UserTokens
            .Where(t => t.IsRevoked || t.ExpiresAt < _maxExpDate)
            .ToListAsync(stoppingToken);

            if (tokensToDelete.Any())
            {
                context.UserTokens.RemoveRange(tokensToDelete);
                await context.SaveChangesAsync(stoppingToken);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Removed {tokensToDelete.Count} revoked tokens");
                Console.ResetColor();
            }
        }
    }
}
