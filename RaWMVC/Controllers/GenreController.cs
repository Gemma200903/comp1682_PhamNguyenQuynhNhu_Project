using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Data;
using RaWMVC.Data.Entities;
using RaWMVC.ViewComponents;
using RaWMVC.ViewModels;

namespace RaWMVC.Controllers
{
    [Authorize(Roles = "Admintrator")]
    public class GenreController : Controller
    {
        private readonly RaWDbContext _context;
        private readonly INotyfService _notyf;

        public GenreController(RaWDbContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Create(GenreViewModel genreVM)
        {
            try
            {
                var existingGenre = await _context.Genres
                                       .FirstOrDefaultAsync(t => t.GenreName == genreVM.GenreName.Trim());

                if (existingGenre != null)
                {
                    //=== If the tag already exists, display an error message ===//
                    _notyf.Warning("Genre name already exists.");


                    //=== Return the view with the existing data to allow the user to correct it ===//
                    return RedirectToAction(nameof(Index), genreVM);
                }

                if (genreVM.GenreName.Length > 75)
                {
                    //=== Nếu độ dài của TagName vượt quá 75 ký tự, hiển thị thông báo cảnh báo ===//
                    _notyf.Warning("Genre name is too long. Please shorten it.");

                    //=== Trả về view với dữ liệu hiện tại để người dùng chỉnh sửa ===//
                    return RedirectToAction(nameof(Index), genreVM);
                }

                if (genreVM.GenreDescription.Length > 200)
                {
                    //=== Nếu độ dài của TagName vượt quá 75 ký tự, hiển thị thông báo cảnh báo ===//
                    _notyf.Warning("Genre description is too long. Please shorten it.");

                    //=== Trả về view với dữ liệu hiện tại để người dùng chỉnh sửa ===//
                    return RedirectToAction(nameof(Index), genreVM);
                }


                var countGenre = await _context.Genres.CountAsync();
                var newGenre = new Genre
                {
                    GenreName = genreVM.GenreName.Trim(),
                    GenreDescription = genreVM.GenreDescription?.Trim(),
                    Position = countGenre + 1,
                };
                _context.Genres.Add(newGenre);
                await _context.SaveChangesAsync();

                _notyf.Success("Genre added successfully");

                return RedirectToAction(nameof(Index));
            }
            catch
            {

                _notyf.Error("Failed to add genre!!!!");

                return View(nameof(Index));
            }
        }

        //GET: GenreController/Edit/5
        public async Task<IActionResult> Edit(Guid idGenre)
        {
            var genreVM = await _context.Genres
                .Where(g => g.GenreId.Equals(idGenre))
                .Select(a => new GenreViewModel
                {
                    GenreId = a.GenreId,
                    GenreName = a.GenreName,
                    GenreDescription = a.GenreDescription,
                })
                .SingleOrDefaultAsync();

            if (genreVM == null) return BadRequest();

            return View(nameof(Index), genreVM);
        }

        //POST: GenreController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Genre genreVM)
        {
            try
            {
                var genre = await _context.Genres.FindAsync(genreVM.GenreId);
                if (genre == null) return BadRequest();

                genre.GenreName = genreVM.GenreName.Trim();
                genre.GenreDescription = genreVM.GenreDescription?.Trim();

                var existingGenre = await _context.Genres
                       .FirstOrDefaultAsync(t => t.GenreName == genreVM.GenreName.Trim());

                if (existingGenre != null)
                {
                    //=== If the tag already exists, display an error message ===//
                    _notyf.Warning("Genre name already exists.");


                    //=== Return the view with the existing data to allow the user to correct it ===//
                    return RedirectToAction(nameof(Index), genreVM);
                }

                if (genreVM.GenreName.Length > 75)
                {
                    //=== Nếu độ dài của TagName vượt quá 75 ký tự, hiển thị thông báo cảnh báo ===//
                    _notyf.Warning("Genre name is too long. Please shorten it.");

                    //=== Trả về view với dữ liệu hiện tại để người dùng chỉnh sửa ===//
                    return RedirectToAction(nameof(Index), genreVM);
                }

                if (genreVM.GenreDescription.Length > 200)
                {
                    //=== Nếu độ dài của TagName vượt quá 75 ký tự, hiển thị thông báo cảnh báo ===//
                    _notyf.Warning("Genre description is too long. Please shorten it.");

                    //=== Trả về view với dữ liệu hiện tại để người dùng chỉnh sửa ===//
                    return RedirectToAction(nameof(Index), genreVM);
                }

                await _context.SaveChangesAsync();

                _notyf.Success("Genre edited successfully");

                return RedirectToAction(nameof(Index), genreVM);
            }
            catch
            {
                _notyf.Success("Failed to edited genre!!!");

                return View(nameof(Index), genreVM);
            }
        }
        // POST: ArtistController/Delete/5
        [HttpPost]
        public async Task<ActionResult> Delete(Guid idGenre)
        {
            var status = false;
            var message = "Not yet implemented!!!";
            try
            {
                //=== Predicate/delgate ===//
                var genre = await _context.Genres
                    .Where(t => t.GenreId.Equals(idGenre))
                    .SingleOrDefaultAsync();

                if (genre != null)
                {
                    //=== Decreasement Position ===//
                    var currentPosition = genre.Position;
                    var listGenre = await _context.Genres
                        .Where(x => x.Position > currentPosition)
                        .ToListAsync();
                    if (listGenre != null && listGenre.Count > 0)
                    {
                        foreach (var item in listGenre)
                        {
                            item.Position -= 1;
                        }
                    }
                    //=== Remove Genre ====//
                    _context.Genres.Remove(genre);
                }
                await _context.SaveChangesAsync();
                message = "Delete genre successfully";
                status = true;
            }
            catch
            {
                message = "Execution error!!!";
            }
            return Json(new { status, message });
        }
        public IActionResult ReloadGenreList(int currentPage = 1)
        {

            return ViewComponent(nameof(GenreList), new { currentPage });
        }
    }
}
