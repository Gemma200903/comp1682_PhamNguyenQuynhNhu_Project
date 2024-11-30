using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Data;
using RaWMVC.Data.Entities;

namespace RaWMVC.Controllers
{
    public class PostController : Controller
    {
        private readonly RaWDbContext _context;
        private readonly UserManager<RaWMVCUser> _userManager;

        public PostController(RaWDbContext context, UserManager<RaWMVCUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Create(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return Json(new { success = false, message = "Post content cannot be empty." });
            }

            var user = await _userManager.GetUserAsync(User);
            var post = new Post
            {
                PostId = Guid.NewGuid(),
                UserId = user.Id,
                Username = user.UserName,
                ProfilePicture = user.ProfilePicture,
                PostContent = content,
                CreateOn = DateTime.Now
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Get all followers of the user
            var followers = await _context.Follows
               .Where(f => f.FolloweeId.ToString() == user.Id)
               .Select(f => f.FollowerId)
               .ToListAsync();

            // Create the profile link
            var profileLink = Url.Action("Index", "Profile", new { userId = post.UserId }, Request.Scheme);

            // Prepare notifications for followers
            foreach (var followerId in followers)
            {
                var notification = new Notification
                {
                    UserId = followerId.ToString(),
                    Username = user.UserName,
                    Message = $"{user.UserName} has posted a new update.",
                    Link = profileLink,
                    CreatedDate = DateTime.Now
                };

                _context.Notifications.Add(notification); // Add notification to the context
            }

            await _context.SaveChangesAsync(); // Save all changes, including notifications

            return Json(new { success = true, message = "The post has been posted on your profile." });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid idPost)
        {
            if (idPost == Guid.Empty)
            {
                return Json(new { success = false, message = "Post ID cannot be empty." });
            }

            // Lấy thông tin người dùng hiện tại
            var user = await _userManager.GetUserAsync(User);

            // Tìm bài viết theo idPost
            var post = await _context.Posts.FindAsync(idPost);

            if (post == null)
            {
                return Json(new { success = false, message = "Post not found." });
            }

            // Kiểm tra quyền: người tạo bài viết hoặc admin
            if (post.UserId != user.Id && !User.IsInRole("Admintrator"))
            {
                return Json(new { success = false, message = "You do not have permission to delete this post." });
            }

            // Xóa bài viết
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "The post has been deleted successfully." });
        }
    }
}
