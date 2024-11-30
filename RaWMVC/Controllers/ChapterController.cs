using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Data;
using RaWMVC.Data.Entities;
using RaWMVC.ViewComponents;
using RaWMVC.ViewModels;
using System.Security.Claims;

namespace RaWMVC.Controllers
{
    public class ChapterController : Controller
    {
        private readonly RaWDbContext _context;
        private readonly UserManager<RaWMVCUser> _userManager;
        private readonly INotyfService _notyf;

        public ChapterController(RaWDbContext context, UserManager<RaWMVCUser> userManager, INotyfService notyf)
        {
            _context = context;
            _userManager = userManager;
            _notyf = notyf;
        }

        [Authorize(Roles = "Author, Admintrator")]
        //=============== Create ===============//
        public ActionResult Create(Guid idStory)
        {
            var ChapterVM = new ChapterViewModel
            {
                StoryId = idStory
            };
            return View(ChapterVM);
        }

        [Authorize(Roles = "Author, Admintrator")]
        [HttpPost]
        public async Task<IActionResult> Create(ChapterViewModel ChapterVM, bool isPublish)
        {
            try
            {
                var countChap = await _context.Chapters.CountAsync();

                // Check if the story exists
                bool storyExists = await _context.Stories.AnyAsync(s => s.StoryId == ChapterVM.StoryId);
                if (!storyExists)
                {
                    return Json(new { success = false, message = "Story not found." });
                }

                var Chapter = new Chapter
                {
                    ChapterId = ChapterVM.ChapterId,
                    ChapterTitle = ChapterVM.ChapterTitle.Trim(),
                    ChapterContent = ChapterVM.ChapterContent,
                    Position = countChap + 1,
                    PublishDate = DateTime.Now,
                    StoryId = ChapterVM.StoryId,
                    IsPublished = isPublish,
                    IsDraft = !isPublish
                };

                _context.Chapters.Add(Chapter);
                await _context.SaveChangesAsync();

                // Nếu chương được xuất bản, thông báo cho tất cả các follower của tác giả
                if (isPublish == true)
                {
                    var story = await _context.Stories
                            .Where(s => s.StoryId == ChapterVM.StoryId)
                            .FirstOrDefaultAsync();

                    // Get all followers of the user
                    var followers = await _context.Follows
                       .Where(f => f.FolloweeId.ToString() == story.UserId)
                       .Select(f => f.FollowerId)
                       .ToListAsync();

                    var chapterLink = Url.Action("Detail", "Story", new { isStory = story.StoryId }, Request.Scheme);

                    // Prepare notifications for followers
                    foreach (var followerId in followers)
                    {
                        var notification = new Data.Entities.Notification
                        {
                            UserId = followerId.ToString(),
                            Username = story.Username,
                            Message = $"New chapter '{Chapter.ChapterTitle}' has been published by {story.Username}.",
                            Link = chapterLink,
                            CreatedDate = DateTime.Now
                        };

                        _context.Notifications.Add(notification);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = isPublish ? "Chapter published successfully." : "Chapter saved as draft successfully." });
            }
            catch (Exception ex)
            {
                // Log exception (ex) if needed
                return Json(new { success = false, message = "An error occurred while adding the Chapter." });
            }
        }

        [Authorize(Roles = "Author, Admintrator")]
        //=============== Edit ===============//
        //GET: Chapter/Edit/idStory=?123
        public async Task<IActionResult> Edit(Guid idChapter)
        {
            var chapter = await _context.Chapters
                .Where(c => c.ChapterId.Equals(idChapter))
                .SingleOrDefaultAsync();

            if (chapter == null) return BadRequest();

            var chapterVM = new ChapterViewModel
            {
                ChapterId = chapter.ChapterId,
                ChapterTitle = chapter.ChapterTitle.Trim(),
                ChapterContent = chapter.ChapterContent,
                PublishDate = chapter.PublishDate,
                StoryId = chapter.StoryId,
                IsPublished = chapter.IsPublished,
                IsDraft = chapter.IsDraft
            };

            return View(nameof(Edit), chapterVM);
        }

        [Authorize(Roles = "Author, Admintrator")]
        //POST: Chapter/Edit/idStory=?123
        [HttpPost]
        public async Task<IActionResult> Edit(ChapterViewModel chapterVM, bool isPublish)
        {
            var chapter = await _context.Chapters.FindAsync(chapterVM.ChapterId);
            if (chapter == null) return NotFound();

            chapter.ChapterTitle = chapterVM.ChapterTitle.Trim();
            chapter.ChapterContent = chapterVM.ChapterContent;
            chapter.PublishDate = DateTime.Now;
            chapter.IsPublished = isPublish; // Thiết lập trạng thái xuất bản
            chapter.IsDraft = !isPublish;

            await _context.SaveChangesAsync();

            // Nếu chương được xuất bản, thông báo cho tất cả các follower của tác giả
            if (isPublish == true)
            {
                var story = await _context.Stories
                        .Where(s => s.StoryId == chapterVM.StoryId)
                        .FirstOrDefaultAsync();

                // Get all followers of the user
                var followers = await _context.Follows
                   .Where(f => f.FolloweeId.ToString() == story.UserId)
                   .Select(f => f.FollowerId)
                   .ToListAsync();

                var chapterLink = Url.Action("Detail", "Story", new { isStory = story.StoryId }, Request.Scheme);

                // Prepare notifications for followers
                foreach (var followerId in followers)
                {
                    var notification = new Data.Entities.Notification
                    {
                        UserId = followerId.ToString(),
                        Username = story.Username,
                        Message = $"New chapter '{chapter.ChapterTitle}' has been published by {story.Username}.",
                        Link = chapterLink,
                        CreatedDate = DateTime.Now
                    };

                    _context.Notifications.Add(notification);
                }
                await _context.SaveChangesAsync();

            }


            return Json(new { success = true, message = "Chapter updated successfully." });
        }

        [Authorize(Roles = "Author, Admintrator")]
        //=============== Delete ===============//
        [HttpPost]
        public async Task<ActionResult> Delete(Guid idChapter)
        {
            var Chapter = false;
            var message = "Not yet implemented!!!";
            try
            {
                //=== Predicate/delgate ===//
                var ChapterVM = await _context.Chapters
                    .Where(t => t.ChapterId.Equals(idChapter))
                    .SingleOrDefaultAsync();

                if (ChapterVM != null)
                {
                    //=== Decreasement Position ===//
                    var currentPosition = ChapterVM.Position;

                    var listChapter = await _context.Chapters
                        .Where(x => x.Position > currentPosition)
                        .ToListAsync();

                    if (listChapter != null && listChapter.Count > 0)
                    {
                        foreach (var item in listChapter)
                        {
                            item.Position -= 1;
                        }
                    }
                    //=== Remove Status ====//
                    _context.Chapters.Remove(ChapterVM);
                }
                await _context.SaveChangesAsync();

                message = "Chapter has been deleted successfully.";
                Chapter = true;
            }
            catch
            {
                message = "Execution error!!!";
            }

            return Json(new { Chapter, message });
        }
        [Authorize(Roles = "Author, Admintrator")]
        public IActionResult ReloadChapterList(Guid idStory)
        {
            return ViewComponent(nameof(ChapterList), new { idStory });
        }

        [Authorize]
        public async Task<IActionResult> Detail(Guid idChapter)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .Include(c => c.Comments)
                .Include(c => c.Likes)
                .Where(c => c.ChapterId == idChapter)
                .Select(c => new ChapterViewModel
                {
                    StoryId = c.Story.StoryId,
                    ChapterId = c.ChapterId,
                    ChapterTitle = c.ChapterTitle,
                    ChapterContent = c.ChapterContent,
                    IsPublished = c.IsPublished,
                    UserId = c.Story.UserId,
                    Username = c.Story.Username,
                    Comments = c.Comments.ToList()
                })
                .FirstOrDefaultAsync();

            var nextChapter = await _context.Chapters
                .Where(c => c.StoryId == chapter.StoryId && c.IsPublished == true && c.ChapterId > chapter.ChapterId)
                .OrderBy(c => c.ChapterId)
                .FirstOrDefaultAsync();

            if (chapter == null || !chapter.IsPublished)
            {
                return NotFound();
            }

            ViewBag.ChapterId = idChapter;

            var commentViewModel = new CommentViewModel
            {
                Comments = chapter.Comments.ToList()
            };

            var likeCount = await _context.Like.CountAsync(l => l.ChapterId == idChapter);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var chapterRead = new ChapterReadCount
            {
                ChapterId = idChapter,
                UserId = userId,
                ReadDate = DateTime.UtcNow
            };

            _context.ChapterReadCounts.Add(chapterRead);
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);

            bool isLiked = false;

            if (user != null)
            {
                isLiked = await _context.Like.AnyAsync(l => l.ChapterId == idChapter && l.UserId == user.Id);
            }

            var chapterReadCounts = await _context.ChapterReadCounts
                .Where(cr => cr.ChapterId != null)
                .GroupBy(cr => cr.ChapterId)
                .Select(g => new
                {
                    ChapterId = g.Key,
                    ReadCount = g.Count()
                })
                .ToListAsync();

            // Lấy các truyện gợi ý
            var suggestedStories = await _context.Stories
                .Include(s => s.Chapters)
                .Take(3)
                .Select(s => new StoryViewModel
                {
                    StoryId = s.StoryId,
                    StoryTitle = s.StoryTitle,
                    StoryDescription = s.StoryDescription,
                    Medium = new Medium
                    {
                        FileName = s.Medium.FileName,
                        Extension = s.Medium.Extension
                    },
                })
                .ToListAsync();

            var idStory = chapter.StoryId;
            var chapterList = await GetChaptersByStoryId(idStory);

            var chapterDetailVM = new ChapterDetailViewModel
            {
                ChapterId = chapter.ChapterId,
                Chapter = chapter,
                SuggestedStories = suggestedStories,
                LikeCount = likeCount,
                IsLiked = isLiked,
                Chapters = chapterList.ToList(),
                ProfilePicture = user.ProfilePicture,
                UserId = chapter.UserId,
                Username = chapter.Username,
                NextChapter = nextChapter,
                CommentVM = new CommentViewModel
                {
                    Comments = chapter.Comments.ToList()
                }
            };

            return View(chapterDetailVM);
        }

        public int GetTotalReadCount(Guid idChapter)
        {
            var chapter = _context.Chapters
                .Where(c => c.ChapterId == idChapter)
                .FirstOrDefault();

            return _context.ChapterReadCounts.Count(cr => cr.ChapterId == chapter.ChapterId);
        }

        private async Task<IEnumerable<ChapterViewModel>> GetChaptersByStoryId(Guid storyId)
        {
            var chapters = await _context.Chapters
                .Where(c => c.StoryId.Equals(storyId))
                .OrderBy(c => c.PublishDate)
                .ToListAsync();

            // Chắc chắn trả về một danh sách, không phải null
            return chapters.Select(c => new ChapterViewModel
            {
                ChapterId = c.ChapterId,
                ChapterTitle = c.ChapterTitle,
                PublishDate = c.PublishDate
            });
        }
    }
}
