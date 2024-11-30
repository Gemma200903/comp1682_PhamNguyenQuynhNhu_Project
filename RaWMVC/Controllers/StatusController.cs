using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using RaWMVC.Data;
using RaWMVC.Data.Entities;
using RaWMVC.ViewComponents;
using RaWMVC.ViewModels;

namespace RaWMVC.Controllers
{
    [Authorize(Roles = "Admintrator")]
    public class StatusController : Controller
    {
        private readonly RaWDbContext _context;
        private readonly INotyfService _notyf;
        public StatusController(RaWDbContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Create(StatusViewModel statusVM)
        {
            try
            {
                var existingStatus = await _context.Status
                                        .FirstOrDefaultAsync(t => t.StatusName == statusVM.StatusName.Trim());

                if (existingStatus != null)
                {
                    //=== If the tag already exists, display an error message ===//
                    _notyf.Warning("Status name already exists.");


                    //=== Return the view with the existing data to allow the user to correct it ===//
                    return RedirectToAction(nameof(Index), statusVM);
                }

                if (statusVM.StatusName.Length > 75)
                {
                    //=== Nếu độ dài của TagName vượt quá 75 ký tự, hiển thị thông báo cảnh báo ===//
                    _notyf.Warning("Tag name is too long. Please shorten it.");

                    //=== Trả về view với dữ liệu hiện tại để người dùng chỉnh sửa ===//
                    return RedirectToAction(nameof(Index), statusVM);
                }

                if (statusVM.StatusDescription.Length > 200)
                {
                    //=== Nếu độ dài của TagName vượt quá 75 ký tự, hiển thị thông báo cảnh báo ===//
                    _notyf.Warning("Status description is too long. Please shorten it.");

                    //=== Trả về view với dữ liệu hiện tại để người dùng chỉnh sửa ===//
                    return RedirectToAction(nameof(Index), statusVM);
                }

                var countStatus = await _context.Status.CountAsync();
                var newSttaus = new Status
                {
                    StatusName = statusVM.StatusName.Trim(),
                    StatusDescription = statusVM.StatusDescription?.Trim(),
                    Position = countStatus + 1,
                };
                //=== The status has just been added to the database ===//
                _context.Status.Add(newSttaus);
                await _context.SaveChangesAsync();

                //=== Display successfully saved message ===//
                _notyf.Success("Status added successfully.");

                //=== Return to continue creating ===//
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                //=== Display faily saved message ===//
                _notyf.Error("Failed to add status.");

                //=== If not successful, check the data again ===//
                return View(nameof(Index), statusVM);
            }
        }
		// GET: StatusController/Edit/5
        public async Task<IActionResult> Edit (Guid idStatus)
        {
            var statusVM = await _context.Status
                .Where(s => s.StatusId.Equals(idStatus))
                .Select(a => new StatusViewModel
                {
                    StatusId = a.StatusId,
                    StatusName = a.StatusName,
                    StatusDescription = a.StatusDescription,
                })
                .SingleOrDefaultAsync();

            if (statusVM == null) return BadRequest();

            return View(nameof(Index), statusVM);
        }
		// POST: StatusController/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Status statusVM)
        {
            try
            {
                var status = await _context.Status.FindAsync(statusVM.StatusId);
                if (status == null) return BadRequest();

                status.StatusName = statusVM.StatusName.Trim();
                status.StatusDescription = statusVM.StatusDescription?.Trim();

                var existingStatus = await _context.Status
                                        .FirstOrDefaultAsync(t => t.StatusName == statusVM.StatusName.Trim());

                if (existingStatus != null)
                {
                    //=== If the tag already exists, display an error message ===//
                    _notyf.Warning("Status name already exists.");


                    //=== Return the view with the existing data to allow the user to correct it ===//
                    return RedirectToAction(nameof(Index), statusVM);
                }

                if (statusVM.StatusName.Length > 75)
                {
                    //=== Nếu độ dài của TagName vượt quá 75 ký tự, hiển thị thông báo cảnh báo ===//
                    _notyf.Warning("Tag name is too long. Please shorten it.");

                    //=== Trả về view với dữ liệu hiện tại để người dùng chỉnh sửa ===//
                    return RedirectToAction(nameof(Index), statusVM);
                }

                if (statusVM.StatusDescription.Length > 200)
                {
                    //=== Nếu độ dài của TagName vượt quá 75 ký tự, hiển thị thông báo cảnh báo ===//
                    _notyf.Warning("Status description is too long. Please shorten it.");

                    //=== Trả về view với dữ liệu hiện tại để người dùng chỉnh sửa ===//
                    return RedirectToAction(nameof(Index), statusVM);
                }

                await _context.SaveChangesAsync();

                _notyf.Success("Edited status successfully.");

                return RedirectToAction(nameof(Index));
			}
			catch
            {
                _notyf.Error("Failed to edit status");

                return View(nameof(Index), statusVM); 
            }
        }

        // POST: ArtistController/Delete/5
        [HttpPost]
        public async Task<ActionResult> Delete(Guid idStatus)
        {
            var status = false;
            var message = "Not yet implemented!!!";
            try
            {
                //=== Predicate/delgate ===//
                var statusVM = await _context.Status
                    .Where(t => t.StatusId.Equals(idStatus))
                    .SingleOrDefaultAsync();

                if (statusVM != null)
                {
                    //=== Decreasement Position ===//
                    var currentPosition = statusVM.Position;
                    var listStatus = await _context.Status
                        .Where(x => x.Position > currentPosition)
                        .ToListAsync();
                    if (listStatus != null && listStatus.Count > 0)
                    {
                        foreach (var item in listStatus)
                        {
                            item.Position -= 1;
                        }
                    }
                    //=== Remove Status ====//
                    _context.Status.Remove(statusVM);
                }
                await _context.SaveChangesAsync();
                message = "Delete status successfully";
                status = true;
            }
            catch
            {
                message = "Execution error!!!";
            }
            return Json(new { status, message });
        }
        public IActionResult ReloadStatusList()
        {

            return ViewComponent(nameof(StatusList));
        }

    }
}
