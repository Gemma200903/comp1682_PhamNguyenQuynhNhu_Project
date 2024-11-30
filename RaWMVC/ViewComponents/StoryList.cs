using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Data;
using RaWMVC.Data.Entities;
using RaWMVC.ViewModels;
using System.Security.Claims;

namespace RaWMVC.ViewComponents
{
    public class StoryList : ViewComponent
    {
        private readonly RaWDbContext _context;
        private readonly UserManager<RaWMVCUser> _userManager;
        public StoryList(RaWDbContext context, UserManager<RaWMVCUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var story = await GetStoryAsync();

            return View(story);
        }

        private async Task<List<StoryViewModel>> GetStoryAsync()
        {
            var user = await _userManager.GetUserAsync((ClaimsPrincipal)User);

            // Lọc stories theo UserId
            var stories = await _context.Stories
                .Include(s => s.Medium)
                .Where(s => s.UserId == user.Id) // Lọc dựa trên UserId
                .ToListAsync();

            var chapterCounts = await _context.Chapters
                .Where(c => stories.Select(s => s.StoryId).Contains(c.StoryId))
                .GroupBy(c => c.StoryId)
                .Select(g => new
                {
                    StoryId = g.Key,
                    PostedCount = g.Count(c => !c.IsDraft),
                    DraftCount = g.Count(c => c.IsDraft)
                })
                .ToListAsync();

            var result = stories.Select(story =>
            {
                var counts = chapterCounts.FirstOrDefault(c => c.StoryId == story.StoryId);
                return new StoryViewModel
                {
                    StoryId = story.StoryId,
                    StoryTitle = story.StoryTitle,
                    Medium = story.Medium,
                    PublishedChapter = counts?.PostedCount ?? 0,
                    DraftChapter = counts?.DraftCount ?? 0,
                    PublishDate = story.PublishDate
                };
            }).ToList();

            return result;
        }
    }
}
