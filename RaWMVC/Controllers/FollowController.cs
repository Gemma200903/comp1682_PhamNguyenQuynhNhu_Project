using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Data;
using RaWMVC.Data.Entities;

namespace RaWMVC.Controllers
{
    public class FollowController : Controller
    {
        private readonly RaWDbContext _context;
        private readonly UserManager<RaWMVCUser> _userManager;
        private readonly INotyfService _notyf;

        public FollowController(RaWDbContext context, UserManager<RaWMVCUser> userManager, INotyfService notyf)
        {
            _context = context;
            _userManager = userManager;
            _notyf = notyf;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFollow(Guid followeeId)
        {
            var status = false;
            var message = "Not yet Implement";
            var followerId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(followerId))
            {
                return Unauthorized();
            }

            var followerGuid = Guid.Parse(followerId);

            // Kiểm tra hành động follow hiện tại
            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerGuid && f.FolloweeId == followeeId);

            if (existingFollow == null)
            {
                // Tạo hành động follow mới
                var follow = new Follow
                {
                    FollowerId = followerGuid,
                    FolloweeId = followeeId,
                    FollowedOn = DateTime.Now
                };

                _context.Follows.Add(follow);
                status = true;
                message = "You are following successfully.";
                // Kiểm tra thông báo gần đây nhất
                var lastNotification = await _context.Notifications
                    .Where(n => n.UserId == followeeId.ToString() && n.Username == User.Identity.Name)
                    .OrderByDescending(n => n.CreatedDate)
                    .FirstOrDefaultAsync();

                if (lastNotification == null ||
                    (DateTime.Now - lastNotification.CreatedDate).TotalMinutes > 30)
                {
                    // Gửi thông báo mới
                    var notification = new Data.Entities.Notification
                    {
                        UserId = followeeId.ToString(),
                        Username = User.Identity.Name,
                        Message = $"{User.Identity.Name} is now following you.",
                        CreatedDate = DateTime.Now
                    };

                    _context.Notifications.Add(notification);
                    TempData["Message"] = "You are now following this user.";
                }
                else
                {
                    TempData["Message"] = "You are now following this user. No new notification was sent.";
                }
            }
            else
            {
                // Hủy follow
                _context.Follows.Remove(existingFollow);
                status = true;
                message = "You are following successfully.";
            }

            await _context.SaveChangesAsync();

            return Json(new { status, message });
        }
    }
}
