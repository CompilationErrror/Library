using LibraryApi.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Authentication
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval;

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
                    _logger.LogInformation("Successfully cleaned up revoked tokens at: {time}", DateTimeOffset.Now);
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
                .Where(t => t.IsRevoked)
                .ToListAsync(stoppingToken);

            if (tokensToDelete.Any())
            {
                context.UserTokens.RemoveRange(tokensToDelete);
                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Removed {count} revoked tokens", tokensToDelete.Count);
            }
        }
    }
}
