﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Data;
using RaWMVC.Data.Entities;

namespace RaWMVC.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly RaWDbContext _context;
        private readonly UserManager<RaWMVCUser> _userManager;

        public ReportController(RaWDbContext context, UserManager<RaWMVCUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> ReportStory(Guid storyId)
        {
            var story = await _context.Stories.FindAsync(storyId);
            if (story == null)
            {
                return NotFound();
            }

            var report = new Report { StoryId = storyId };
            return PartialView(report);
        }

        [HttpPost]
        public async Task<IActionResult> ReportStory(Guid storyId, string reason, string description)
        {
            var userReport = await _userManager.GetUserAsync(User);

            var story = await _context.Stories
               .Where(s => s.StoryId == storyId)
               .Select(s => new { s.UserId, s.Username })
               .FirstOrDefaultAsync();

            var report = new Report
            {
                StoryId = storyId,
                AuthorId = story.UserId,
                AuthorName = story.Username,
                UserId = userReport.Id,
                Username = userReport.UserName,
                Reason = reason,
                Description = description,
                ReportDate = DateTime.Now,
                IsReviewed = false,
                IsApproved = false
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            TempData["MessageSuccess"] = "Your report has been submitted successfully.";
            return RedirectToAction("Detail", "Story", new { idStory = storyId });
        }

        [Authorize(Roles = "Admintrator")]
        public async Task<IActionResult> ManageReports()
        {
            var reports = await _context.Reports
                .Include(r => r.Story)
                .Where(r => !r.IsReviewed)
                .ToListAsync();

            return View(reports);
        }

        [Authorize(Roles = "Admintrator")]
        [HttpPost]
        public async Task<IActionResult> ApproveReport(Guid reportId)
        {
            var report = await _context.Reports
                .Include(r => r.Story)
                .Where(r => r.ReportId.Equals(reportId))
                .FirstOrDefaultAsync();

            if (report != null)
            {
                report.IsReviewed = true;
                report.IsApproved = true;
                await _context.SaveChangesAsync();

                //=== Save information to delete story after 24 hours ===//
                var scheduledDelete = new ScheduledDelete
                {
                    StoryId = report.StoryId,
                    ScheduledTime = DateTime.Now.AddHours(24)
                };

                _context.ScheduledDeletes.Add(scheduledDelete);
                await _context.SaveChangesAsync();

                //=== Create notification for reporter ===//
                var notificationForReporter = new Notification
                {
                    UserId = report.UserId,
                    Username = report.Username,
                    Message = $"Your report on '{report.Story.StoryTitle}' has been approved. The story will be removed in 3 days.",
                    Link = $"/Story/Detail?idStory={report.StoryId}",
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };

                //=== Create a notification for the story author ===//
                var notificationForAuthor = new Notification
                {
                    UserId = report.AuthorId,
                    Username = report.Story?.Username,
                    Message = $"Your story '{report.Story.StoryTitle}' has been reported and will be removed in 3 days.",
                    Link = $"/Story/Detail?idStory={report.StoryId}",
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };

                //=== Save notifications to database ===//
                _context.Notifications.Add(notificationForReporter);
                _context.Notifications.Add(notificationForAuthor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageReports");
        }

        [Authorize(Roles = "Admintrator")]
        [HttpPost]
        public async Task<IActionResult> RejectReport(Guid reportId)
        {
            var report = await _context.Reports
                .Include(r => r.Story)
                .Where(r => r.ReportId.Equals(reportId))
                .FirstOrDefaultAsync();

            if (report != null)
            {
                report.IsReviewed = true;
                report.IsApproved = false;
                await _context.SaveChangesAsync();

                var notification = new Notification
                {
                    UserId = report.UserId,
                    Username = report.Username,
                    Message = $"Your report on '{report.Story.StoryTitle}' has been rejected.",
                    Link = "/Story/Detail/" + report.StoryId,
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageReports");
        }
    }
}
