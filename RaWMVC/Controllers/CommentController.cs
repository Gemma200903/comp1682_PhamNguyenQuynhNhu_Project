using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Data;
using RaWMVC.Data.Entities;

namespace RaWMVC.Controllers
{
    public class CommentController : Controller
    {
        private RaWDbContext _context;
        private UserManager<RaWMVCUser> _userManager;
        private readonly INotyfService _notyf;

        public CommentController(RaWDbContext context, UserManager<RaWMVCUser> userManager, INotyfService notyf)
        {
            _context = context;
            _userManager = userManager;
            _notyf = notyf;
        }

        [HttpPost]
        public async Task<IActionResult> Create(string content, Guid chapterId)
        {
            if (string.IsNullOrEmpty(content))
            {
                _notyf.Warning("Comment content cannot empty");

                return RedirectToAction("Detail", "Chapter", new { idChapter = chapterId });
            }

            var user = await _userManager.GetUserAsync(User);
            var chapter = await _context.Chapters   
                        .Where(c => c.ChapterId == chapterId)
                        .Include(c => c.Story)
                        .FirstOrDefaultAsync();

            if (chapter == null)
            {
                _notyf.Warning("Chapter is null");

                return RedirectToAction("Detail", "Chapter", new { idChapter = chapterId });
            }

            // Create new comment
            var comment = new Comment
            {
                CommentId = Guid.NewGuid(),
                UserId = user.Id,
                Username = user.UserName,
                ProfilePicture = user.ProfilePicture,
                CommentContent = content,
                ChapterId = chapter.ChapterId,
                CreateOn = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            if(chapter.Story.UserId != user.Id)
            {
                // Create notification for the story owner
                var notification = new Data.Entities.Notification
                {
                    NotificationId = Guid.NewGuid(),
                    UserId = chapter.Story.UserId,  // Story owner
                    Username = user.UserName,
                    Message = $"{user.UserName} commented on your chapter '{chapter.ChapterTitle}' in the story '{chapter.Story.StoryTitle}'",
                    CreatedDate = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            // Return updated comments list
            return RedirectToAction("Detail", "Chapter", new { idChapter = chapterId });
        }

        private async Task<List<Comment>> GetCommentsForChapter(Guid chapterId)
        {
            return await _context.Comments
                        .Where(c => c.ChapterId == chapterId)
                        .OrderByDescending(c => c.CreateOn)
                        .ToListAsync();
        }

        public async Task<IActionResult> Delete(Guid idComment, Guid chapterId)
        {
            bool isDeleted = false;
            string message = "You do not have permission to delete this comment.";

            try
            {
                // === Retrieve Comment === //
                var comment = await _context.Comments
                    .Where(c => c.ChapterId.Equals(chapterId))
                    .SingleOrDefaultAsync(c => c.CommentId == idComment);

                if (comment != null)
                {
                    // Get current user
                    var user = await _userManager.GetUserAsync(User);

                    var chapter = await _context.Chapters
                        .Where(c => c.ChapterId == chapterId)
                        .Include(c => c.Story)
                        .FirstOrDefaultAsync();

                    if (chapter == null)
                    {
                        message = "Chapter not found.";
                        return Json(new { isDeleted, message, chapterId });
                    }

                    // === Check permissions === //
                    bool isAuthor = chapter.Story.UserId == user.Id;
                    bool isCommenter = comment.UserId == user.Id;
                    bool isAdmin = await _userManager.IsInRoleAsync(user, "Admintrator"); 

                    if (isAuthor || isCommenter || isAdmin)
                    {
                        // === Remove Comment === //
                        _context.Comments.Remove(comment);
                        await _context.SaveChangesAsync();

                        isDeleted = true;
                        message = "Comment deleted successfully.";
                    }
                }
                else
                {
                    message = "Comment not found.";
                }
            }
            catch
            {
                message = "An error occurred while deleting the comment.";
            }

            return Json(new { isDeleted, message, chapterId });
        }
    }
}
