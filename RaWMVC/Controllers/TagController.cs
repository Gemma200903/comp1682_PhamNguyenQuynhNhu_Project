using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RaWMVC.Data;
using RaWMVC.Data.Entities;
using RaWMVC.ViewComponents;
using RaWMVC.ViewModels;

namespace RaWMVC.Controllers
{
    [Authorize(Roles = "Admintrator")]
    public class TagController : Controller
    {
        private readonly RaWDbContext _context;
        private readonly INotyfService _notyf;
        public TagController(RaWDbContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }
        public IActionResult Index()
        {
            return View();
        }
        // POST: TagController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TagViewModel tagVM)
        {
            try
            {
                var existingTag = await _context.Tags
                                        .FirstOrDefaultAsync(t => t.TagName == tagVM.TagName.Trim());

                if (existingTag != null)
                {
                    //=== If the tag already exists, display an error message ===//
                    _notyf.Warning("Tag name already exists.");


                    //=== Return the view with the existing data to allow the user to correct it ===//
                    return RedirectToAction(nameof(Index), tagVM);
                }

                if (tagVM.TagName.Length > 75)
                {
                    //=== Nếu độ dài của TagName vượt quá 75 ký tự, hiển thị thông báo cảnh báo ===//
                    _notyf.Warning("Tag name is too long. Please shorten it.");

                    //=== Trả về view với dữ liệu hiện tại để người dùng chỉnh sửa ===//
                    return RedirectToAction(nameof(Index), tagVM);
                }

                if (tagVM.TagDescription.Length > 200)
                {
                    //=== Nếu độ dài của TagName vượt quá 75 ký tự, hiển thị thông báo cảnh báo ===//
                    _notyf.Warning("Tag description is too long. Please shorten it.");

                    //=== Trả về view với dữ liệu hiện tại để người dùng chỉnh sửa ===//
                    return RedirectToAction(nameof(Index), tagVM);
                }

                var countTag = await _context.Tags.CountAsync();

                var newTag = new Tag
                {
                    TagName = tagVM.TagName.Trim(),
                    TagDescription = tagVM.TagDescription?.Trim(),
                    Position = countTag + 1
                };
                //=== The tag has just been added to the database ===//
                _context.Tags.Add(newTag);
                await _context.SaveChangesAsync();

                //=== Display successfully saved message ===//
                _notyf.Success("Tag added successfully.");

                //=== Return to continue creating ===//
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                //=== Display faily saved message ===//
                _notyf.Error("Failed to add tag.");

                //=== If not successful, check the data again ===//
                return View(nameof(Index));
            }
        }

        // GET: TagController/Edit/5
        public async Task<IActionResult> Edit(Guid idTag)
        {
            var tagVM = await _context.Tags
                .Where(s => s.TagId.Equals(idTag))
                .Select(a => new TagViewModel
                {
                    TagId = a.TagId,
                    TagName = a.TagName,
                    TagDescription = a.TagDescription,
                })
                .SingleOrDefaultAsync();

            if (tagVM == null) return BadRequest();

            return View(nameof(Index), tagVM);
        }

        // POST: TagController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tag tagVM)
        {
            try
            {
                var tag = await _context.Tags.FindAsync(tagVM.TagId);
                if (tag == null) return BadRequest();

                tag.TagName = tagVM.TagName.Trim();
                tag.TagDescription = tagVM.TagDescription?.Trim();

                var existingTag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.TagName == tagVM.TagName.Trim());
                if (existingTag != null)
                {
                    //=== If the tag already exists, display an error message ===//
                    _notyf.Warning("Tag name already exists.");


                    //=== Return the view with the existing data to allow the user to correct it ===//
                    return RedirectToAction(nameof(Index), tagVM);
                }

                await _context.SaveChangesAsync();

                _notyf.Success("Edited tag successfully.");

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                _notyf.Success("Failed to edit tag");

                return View(nameof(Index), tagVM);
            }
        }

        // POST: ArtistController/Delete/5
        [HttpPost]
        public async Task<ActionResult> Delete(Guid idTag)
        {
            var status = false;
            var message = "Not yet implemented!!!";
            try
            {
                //=== Predicate/delgate ===//
                var tag = await _context.Tags
                    .Where(t => t.TagId.Equals(idTag))
                    .SingleOrDefaultAsync();

                if (tag != null)
                {
                    //=== Decreasement Position ===//
                    var currentPosition = tag.Position;
                    var listTag = await _context.Tags
                        .Where(x => x.Position > currentPosition)
                        .ToListAsync();
                    if (listTag != null && listTag.Count > 0)
                    {
                        foreach (var item in listTag)
                        {
                            item.Position -= 1;
                        }
                    }
                    //=== Remove Tag ====//
                    _context.Tags.Remove(tag);
                }
                await _context.SaveChangesAsync();
                message = "Tag deleted successfully.";
                status = true;
            }
            catch
            {
                message = "Execution error!!!";
            }
            return Json(new { status, message });
        }
        public IActionResult ReloadTagList(int currentPage = 1)
        {
            return ViewComponent(nameof(TagList), new { currentPage });
        }
    }
}
