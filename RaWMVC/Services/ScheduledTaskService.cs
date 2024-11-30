using Microsoft.EntityFrameworkCore;
using RaWMVC.Data;

public class StoryDeletionService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StoryDeletionService> _logger;

    public StoryDeletionService(IServiceScopeFactory scopeFactory, ILogger<StoryDeletionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("StoryDeletionService started.");
        // Thiết lập Timer để thực hiện công việc mỗi giờ
        _timer = new Timer(async _ => await DeleteScheduledStoriesAsync(), null, TimeSpan.Zero, TimeSpan.FromHours(1));
        return Task.CompletedTask;
    }

    private async Task DeleteScheduledStoriesAsync()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<RaWDbContext>();
                var now = DateTime.UtcNow; // Sử dụng UTC để tránh lỗi múi giờ
                var scheduledDeletes = await context.ScheduledDeletes
                    .Where(sd => sd.ScheduledTime <= now)
                    .ToListAsync();

                if (scheduledDeletes.Any())
                {
                    _logger.LogInformation($"Found {scheduledDeletes.Count} stories scheduled for deletion.");
                }

                foreach (var scheduledDelete in scheduledDeletes)
                {
                    var story = await context.Stories.FindAsync(scheduledDelete.StoryId);
                    if (story != null)
                    {
                        context.Stories.Remove(story);
                        _logger.LogInformation($"Story with ID {story.StoryId} has been deleted.");
                    }
                    else
                    {
                        _logger.LogWarning($"Story with ID {scheduledDelete.StoryId} not found.");
                    }

                    context.ScheduledDeletes.Remove(scheduledDelete);
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Scheduled stories deletion process completed.");
            }
            catch
            {
                _logger.LogError("An error occurred during story deletion.");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("StoryDeletionService stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}