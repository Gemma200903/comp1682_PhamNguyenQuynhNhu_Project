using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Data;
using RaWMVC.Data.Entities;

namespace RaWMVC.Controllers
{
    public class ReplyController : Controller
    {
        private readonly RaWDbContext _context;
        private readonly UserManager<RaWMVCUser> _userManager;
        private readonly INotyfService _notyf;

        public ReplyController(RaWDbContext context, UserManager<RaWMVCUser> userManager, INotyfService notyf)
        {
            _context = context;
            _userManager = userManager;
            _notyf = notyf;
        }
        [HttpPost]
        public async Task<IActionResult> Create(Guid postId, string content)
        {
            var user = await _userManager.GetUserAsync(User);

            if (string.IsNullOrWhiteSpace(content))
            {
                _notyf.Error("Comment content cannot be left blank.");
                return RedirectToAction("Index", "Profile", new { userId = user.Id });
            }

            if (content.Length > 200)
            {
                _notyf.Error("Comment content is too long. Please shorten it.");
                return RedirectToAction("Index", "Profile", new { userId = user.Id });
            }

            var postExists = await _context.Posts
                .Where(p => p.PostId == postId)
                .Select(p => new { p.UserId }) // Get the userId of the post owner
                .FirstOrDefaultAsync();

            if (postExists == null)
            {
                _notyf.Error("Post does not exist.");
                return RedirectToAction("Index", "Profile", new { userId = postExists.UserId });
            }

            var reply = new Reply
            {
                ReplyId = Guid.NewGuid(),
                PostId = postId,
                ReplyContent = content,
                CreateAt = DateTime.Now,
                UserId = user.Id,
                ProfilePicture = user.ProfilePicture,
                Username = user.UserName
            };

            _context.Replies.Add(reply);

            // Create a notification for the post owner
            var notification = new Data.Entities.Notification
            {
                UserId = postExists.UserId,
                Username = user.UserName,
                Message = $"{user.UserName} replied to your post.",
                CreatedDate = DateTime.Now
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();
            _notyf.Success("Reply added successfully.");

            return RedirectToAction("Index", "Profile", new { userId = postExists.UserId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid idPost, Guid idReply)
        {
            bool isDeleted = false;
            string message = "Comment not found.";

            try
            {
                // Tìm reply dựa trên idPost và idReply
                var reply = await _context.Replies
                    .Where(c => c.PostId == idPost && c.ReplyId == idReply)
                    .SingleOrDefaultAsync();

                if (reply != null)
                {
                    // Lấy người dùng hiện tại
                    var currentUser = await _userManager.GetUserAsync(User);

                    // Lấy thông tin bài viết để kiểm tra quyền
                    var post = await _context.Posts
                        .Where(p => p.PostId == idPost)
                        .SingleOrDefaultAsync();

                    // Kiểm tra quyền: người tạo reply, admin, hoặc người tạo bài viết
                    if (reply.UserId == currentUser.Id ||
                        User.IsInRole("Admintrator") ||
                        (post != null && post.UserId == currentUser.Id))
                    {
                        _context.Replies.Remove(reply);
                        await _context.SaveChangesAsync();

                        isDeleted = true;
                        message = "Comment deleted successfully.";
                    }
                    else
                    {
                        message = "You do not have permission to delete this comment.";
                    }
                }
            }
            catch (Exception ex)
            {
                message = $"An error occurred while deleting the comment: {ex.Message}";
            }

            return Json(new { isDeleted, message });
        }
    }
}
