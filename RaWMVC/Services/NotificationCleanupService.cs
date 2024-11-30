using Microsoft.EntityFrameworkCore;
using RaWMVC.Data;

public class NotificationCleanupService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IServiceScopeFactory _scopeFactory;

    public NotificationCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Thiết lập Timer để thực hiện công việc mỗi ngày
        _timer = new Timer(async _ => await DeleteOldNotificationsAsync(), null, TimeSpan.Zero, TimeSpan.FromDays(1));
        return Task.CompletedTask;
    }

    private async Task DeleteOldNotificationsAsync()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<RaWDbContext>();

                // Xác định thời điểm cách đây 15 ngày
                var cutoffDate = DateTime.UtcNow.AddDays(-15);

                // Xóa trực tiếp trong database mà không tải dữ liệu vào bộ nhớ
                int deletedCount = await context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM Notifications WHERE CreatedDate <= {0}", cutoffDate);

                // Log số lượng thông báo đã xóa (tuỳ chỉnh log framework nếu cần)
                Console.WriteLine($"Deleted {deletedCount} old notifications.");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để theo dõi
                Console.Error.WriteLine($"Error occurred while cleaning notifications: {ex.Message}");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Dừng Timer khi dịch vụ dừng
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}