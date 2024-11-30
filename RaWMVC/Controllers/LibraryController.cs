using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Data;
using RaWMVC.Data.Entities;

namespace RaWMVC.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly RaWDbContext _context;
        private readonly ILogger _logger;
        private readonly UserManager<RaWMVCUser> _userManager;

        public LibraryController(RaWDbContext context, UserManager<RaWMVCUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            // Check if Libraries DbSet is initialized
            if (_context.Libraries == null)
            {
                throw new Exception("Libraries table is not initialized in the DbContext");
            }

            // Eagerly load related entities like Story and Medium
            var libraries = await _context.Libraries
                       .Where(l => l.UserId == user.Id)
                       .Include(l => l.Story) // Bao gồm Story
                       .ThenInclude(l => l.Medium)
                       .ToListAsync();

            return View(libraries);
        }

        [HttpPost]
        public async Task<IActionResult> AddAndRemoveLibrary(Guid storyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }
            try
            {
                var existingLibrary= await _context.Libraries
                                .FirstOrDefaultAsync(l => l.StoryId == storyId && l.UserId == user.Id);

                if (existingLibrary != null)
                {
                    _context.Libraries.Remove(existingLibrary);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Story removed from your library." });
                }
                else
                {
                    var newLibraryEntry = new Library
                    {
                        Id = Guid.NewGuid().ToString(),
                        StoryId = storyId,
                        UserId = user.Id,
                        InMyLibrary = true,
                    };
                    await _context.Libraries.AddAsync(newLibraryEntry);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Story added to your library." });
                }
            }
            catch
            {
                return StatusCode(500, "Internal server error while processing your request.");
            }
        }
    }
}