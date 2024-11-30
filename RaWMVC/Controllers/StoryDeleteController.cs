using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Data;
using RaWMVC.ViewModels;

public class StoryDeleteController : Controller
{
    private readonly RaWDbContext _context;

    public StoryDeleteController(RaWDbContext context)
    {
        _context = context;
    }

    // Hiển thị danh sách truyện sắp bị xóa
    public async Task<IActionResult> Index()
    {
        var scheduledDeletes = await _context.ScheduledDeletes
            .Include(sd => sd.Stories)
            .ToListAsync();

        var viewModel = scheduledDeletes.Select(sd => new ScheduledDeleteViewModel
        {
            StoryId = sd.StoryId,
            StoryTitle = sd.Stories.StoryTitle,
            AuthorName = sd.Stories.Username,
            ApprovedTime = sd.ScheduledTime.AddDays(-3),
            ScheduledDeleteTime = sd.ScheduledTime
        }).ToList();

        return View(viewModel);
    }

    // Xóa lịch xóa truyện
    [HttpPost]
    public async Task<IActionResult> CancelDelete(Guid storyId)
    {
        var scheduledDelete = await _context.ScheduledDeletes.FirstOrDefaultAsync(sd => sd.StoryId == storyId);

        if (scheduledDelete == null)
        {
            return NotFound("Scheduled delete record not found.");
        }

        _context.ScheduledDeletes.Remove(scheduledDelete);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}
