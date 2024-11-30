using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Commons;
using RaWMVC.Data;
using RaWMVC.Data.Entities;
using RaWMVC.Enums;
using RaWMVC.ViewComponents;
using RaWMVC.ViewModels;

namespace RaWMVC.Controllers
{
    public class StoryController : Controller
    {
        private readonly UserManager<RaWMVCUser> _userManager;
        private readonly RaWDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly INotyfService _notyf;

        public StoryController(RaWDbContext context, IWebHostEnvironment environment, UserManager<RaWMVCUser> userManager, INotyfService notyf)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
            _notyf = notyf;
        }
        [Authorize(Roles = "Author")]
        //=============== Index ===============//
        public async Task<IActionResult> Index()
        {
            var story = await _context.Stories
                .Include(s => s.Chapters)
                .Include(s => s.Medium)
                .OrderBy(s => s.Position)
                .ToListAsync();

            return View(story);
        }

        [Authorize(Roles = "Author")]
        //=============== Create ===============//
        public async Task<IActionResult> Create()
        {
            //ViewBag.Tags = await GetTagsSelectList();
            //ViewBag.Status = await GetStatusSelectList();
            //ViewBag.Genres = await GetGenreSelectList();
            await PopulateViewBagsAsync();

            return PartialView(nameof(Create));
        }

        [Authorize(Roles = "Author")]
        [HttpPost]
        public async Task<IActionResult> Create(StoryViewModel storyVM)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                // Save the uploaded file
                var fileImage = await SaveMedia(storyVM.CoverImage);

                if (fileImage == null)
                {
                    await PopulateViewBagsAsync();
                    return Json(new { success = false, messageFail = "You must add a file cover image." });
                }

                var newStory = new Story
                {
                    StoryTitle = storyVM.StoryTitle.Trim(),
                    StoryDescription = storyVM.StoryDescription?.Trim(),
                    StatusId = storyVM.StatusId,
                    TagId = storyVM.TagId,
                    GenreId = storyVM.GenreId,
                    PublishDate = DateTime.Now,
                    UserId = user.Id,
                    Username = user.UserName,
                    MediumId = fileImage.Id, // Link to the image file in your database
                    Position = await _context.Stories.CountAsync() + 1
                };

                _context.Stories.Add(newStory);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Story added successfully" });
            }
            catch
            {
                await PopulateViewBagsAsync();
                return Json(new { success = false, message = "Failed to add story." });
            }
        }

        [Authorize(Roles = "Author")]
        //=============== Edit ===============//
        //GET: StoryController/Edit/27
        public async Task<IActionResult> Edit(Guid idStory)
        {
            var story = await _context.Stories
                .Where(s => s.StoryId.Equals(idStory))
                .Include(s => s.Chapters)
                .Include(s => s.Medium)
                .SingleOrDefaultAsync();

            if (story == null) return BadRequest();

            var storyVM = new StoryViewModel
            {
                StoryId = story.StoryId,
                StoryTitle = story.StoryTitle,
                StoryDescription = story.StoryDescription,
                TagId = story.TagId,
                GenreId = story.GenreId,
                StatusId = story.StatusId,
            };

            var urlImage = Url.Content($"~/storyImg/{story.Medium?.FileName}.{story.Medium?.Extension}");
            ViewBag.CoverImage = urlImage;

            await PopulateViewBagsAsync();

            return PartialView(nameof(Edit), storyVM);
        }

        [Authorize(Roles = "Author")]
        //POST: StoryController/Edit/12
        [HttpPost]
        public async Task<IActionResult> Edit(StoryViewModel storyVM)
        {
            try
            {
                var story = await _context.Stories.FindAsync(storyVM.StoryId);

                if (story == null) return BadRequest();

                var fileImage = storyVM.CoverImage != null ? await SaveMedia(storyVM.CoverImage) : null;

                if (fileImage == null && story.MediumId == null)
                {
                    // Nếu không có file ảnh mới và câu chuyện không có ảnh cũ, trả về lỗi
                    await PopulateViewBagsAsync();
                    return Json(new { success = false, messageFail = "You must add a cover image." });
                }

                story.StoryTitle = storyVM.StoryTitle;
                story.StoryDescription = storyVM.StoryDescription;
                story.GenreId = storyVM.GenreId;
                story.TagId = storyVM.TagId;
                story.StatusId = storyVM.StatusId;

                if (fileImage != null)
                {
                    story.MediumId = fileImage.Id;
                }

                _context.Update(story);
                await _context.SaveChangesAsync();

                return Json(new { success = false, message = "Story edited successfully" });
            }
            catch
            {

                await PopulateViewBagsAsync();
                return Json(new { success = false, message = "Failed to edited story" });
            }

        }

        [Authorize(Roles = "Author")]
        //=============== Delete ===============//
        // POST: StoryController/Delete/5
        [HttpPost]
        public async Task<ActionResult> Delete(Guid idStory)
        {
            var story = false;
            var message = "Not yet implemented!!!";
            try
            {
                //=== Predicate/delgate ===//
                var storyVM = await _context.Stories
                    .Where(t => t.StoryId.Equals(idStory))
                    .SingleOrDefaultAsync();

                if (storyVM != null)
                {
                    //=== Decreasement Position ===//
                    var currentPosition = storyVM.Position;
                    var listStory = await _context.Stories
                        .Where(x => x.Position > currentPosition)
                        .ToListAsync();

                    if (listStory != null && listStory.Count > 0)
                    {
                        foreach (var item in listStory)
                        {
                            item.Position -= 1;
                        }
                    }
                    //=== Remove Status ====//
                    _context.Stories.Remove(storyVM);
                }
                await _context.SaveChangesAsync();
                message = "Delete story successfully";
                story = true;
            }
            catch
            {
                message = "Execution error!!!";
            }

            return Json(new { story, message });
        }

        [Authorize(Roles = "Author")]
        public IActionResult ReloadStoryList()
        {

            return ViewComponent(nameof(StoryList));
        }
        //=============== Detail ===============//
        public async Task<IActionResult> Detail(Guid idStory)
        {
            // Lấy thông tin câu chuyện
            var story = await _context.Stories
                .Include(s => s.Chapters)
                .Include(s => s.Medium)
                .Include(s => s.Status)
                .Include(s => s.Genre)
                .Include(s => s.Tag)
                .Where(s => s.StoryId.Equals(idStory))
                .FirstOrDefaultAsync();

            if (story == null)
            {
                return NotFound("Story not found.");
            }

            // Lấy danh sách ID của các chapter
            var chapterIds = story.Chapters.Select(c => c.ChapterId).ToList();

            // Đếm số lượt đọc của từng chapter
            var chapterReadCounts = await _context.ChapterReadCounts
                .Where(cr => chapterIds.Contains(cr.ChapterId))
                .GroupBy(cr => cr.ChapterId)
                .Select(g => new
                {
                    ChapterId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Đếm tổng số lượt thích của từng chapter
            var chapterLikeCounts = await _context.Like
                .Where(l => chapterIds.Contains(l.ChapterId))
                .GroupBy(l => l.ChapterId)
                .Select(g => new
                {
                    ChapterId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Lấy tổng số lượt đọc và lượt thích
            var totalReadCount = chapterReadCounts.Sum(c => c.Count);
            var totalLikeCount = chapterLikeCounts.Sum(c => c.Count);

            // Lấy thông tin người dùng hiện tại
            var user = await _userManager.GetUserAsync(User);

            // Lấy danh sách đề xuất câu chuyện
            var suggestStories = await _context.Stories
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

            // Map dữ liệu sang ViewModel
            var storyVM = new StoryViewModel
            {
                StoryId = story.StoryId,
                StoryTitle = story.StoryTitle,
                StoryDescription = story.StoryDescription,
                SuggestedStories = suggestStories,
                TotalReadCount = totalReadCount,
                TotalLikeCount = totalLikeCount,
                UserId = story.UserId,
                Username = story.Username,
                ProfilePicture = user?.ProfilePicture ?? string.Empty,
                StatusName = story.Status.StatusName,
                GenreName = story.Genre.GenreName,
                TagName = story.Tag.TagName,
                Medium = new Medium
                {
                    FileName = story.Medium.FileName,
                    Extension = story.Medium.Extension
                },
                Chapters = story.Chapters
                    .Where(c => c.IsPublished)
                    .Select(c => new ChapterViewModel
                    {
                        ChapterId = c.ChapterId,
                        ChapterTitle = c.ChapterTitle,
                        Position = c.Position,
                        PublishDate = c.PublishDate,
                        IsPublished = c.IsPublished,
                        // Tính số lượt đọc và lượt thích cho từng chapter
                        ReadCount = chapterReadCounts.FirstOrDefault(rc => rc.ChapterId == c.ChapterId)?.Count ?? 0,
                        LikeCount = chapterLikeCounts.FirstOrDefault(lc => lc.ChapterId == c.ChapterId)?.Count ?? 0
                    })
                    .OrderBy(c => c.PublishDate)
                    .ToList()
            };

            return View(storyVM);
        }


        [Authorize(Roles = "Author")]
        //=============== List Chapter ===============//
        public async Task<IActionResult> ListChapter(Guid idStory)
        {
            if (idStory == Guid.Empty)
            {
                return BadRequest("Invalid story ID.");
            }

            var story = await _context.Stories
                .Include(s => s.Chapters)
                .Where(s => s.StoryId.Equals(idStory))
                .Select(s => new StoryViewModel
                {
                    StoryId = s.StoryId,
                    StoryTitle = s.StoryTitle,
                    Chapters = s.Chapters.Select(c => new ChapterViewModel
                    {
                        ChapterId = c.ChapterId,
                        ChapterTitle = c.ChapterTitle,
                        Position = c.Position,
                        PublishDate = c.PublishDate,
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (story == null) return BadRequest();

            return View(story);
        }

        //=============== Get Data List ===============//
        private async Task<SelectList> GetTagsSelectList()
        {
            var listTag = await _context.Tags
                .Select(t => new
                {
                    TagId = t.TagId,
                    TagName = t.TagName,
                })
                .ToListAsync();
            return new SelectList(listTag, "TagId", "TagName");
        }
        private async Task<SelectList> GetStatusSelectList()
        {
            var listStatus = await _context.Status
                .Select(s => new
                {
                    StatusId = s.StatusId,
                    StatusName = s.StatusName,
                })
                .ToListAsync();
            return new SelectList(listStatus, "StatusId", "StatusName");
        }
        private async Task<SelectList> GetGenreSelectList()
        {
            var listGenre = await _context.Genres
                .Select(t => new
                {
                    GenreId = t.GenreId,
                    GenreName = t.GenreName,
                })
                .ToListAsync();
            return new SelectList(listGenre, "GenreId", "GenreName");
        }
        private async Task PopulateViewBagsAsync()
        {
            ViewBag.Tags = await GetTagsSelectList();
            ViewBag.Status = await GetStatusSelectList();
            ViewBag.Genres = await GetGenreSelectList();
        }

        //=============== Save Media ===============//
        private async Task<Medium> SaveMedia(IFormFile? file)
        {
            bool isOk = false;
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "You must to add file cover image";
                return null;
            }

            var fileGuidName = Guid.NewGuid().ToString();
            var fileName = "";
            var fileExtension = "";
            var fileNameString = file.FileName;

            if (string.IsNullOrEmpty(fileNameString))
            {
                TempData["Message"] = "Invalid file name.";
                return null;
            }

            try
            {
                string[] arrayExtension = fileNameString.Split('.');
                var fullFileName = "";
                if (arrayExtension != null && arrayExtension.Length > 0)
                {
                    for (int i = 0; i < arrayExtension.Length; i++)
                    {
                        var ext = arrayExtension[i];
                        if (Constants.Invalid_Extenstion.Contains(ext))
                        {
                            return null;
                        }
                    }
                    fileName = arrayExtension[0];
                    fileExtension = arrayExtension[arrayExtension.Length - 1];
                    //=== Check if the file is valid ===//
                    if (!Constants.Valid_Extenstion.Contains(fileExtension))
                    {
                        return null;
                    }
                    fullFileName = fileGuidName + "." + fileExtension;
                }
                var webRoot = _environment.WebRootPath.Normalize();
                var physicalMusicPath = Path.Combine(webRoot, "storyImg/");

                if (!Directory.Exists(physicalMusicPath))
                {
                    Directory.CreateDirectory(physicalMusicPath);
                }
                var physicalPath = Path.Combine(physicalMusicPath, fullFileName);
                using (var stream = System.IO.File.Create(physicalPath))
                {
                    await file.CopyToAsync(stream);
                }
                // === Tạo media ===//
                var count = await _context.Media.CountAsync();
                var newMedium = new Medium
                {
                    Name = fileName,//Tên tìm kiếm
                    FileName = fileGuidName,// Tên lưu trữ
                    Extension = fileExtension,
                    Type = MediaTypeEnum.Audio,
                    Position = count + 1,
                };
                _context.Media.Add(newMedium);
                isOk = true;
                return newMedium;
            }
            catch
            {
            }
            return null;
        }
    }
}
