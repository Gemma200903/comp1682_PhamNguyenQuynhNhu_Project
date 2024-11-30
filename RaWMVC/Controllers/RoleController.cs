using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.ViewComponents;
using RaWMVC.ViewModels;

namespace RaWMVC.Controllers
{
    [Authorize(Roles = "Admintrator")]
    public class RoleController : Controller
    {
        private readonly RoleManager<RaWMVCRole> _roleManager;
        private readonly INotyfService _notyf;

        public RoleController(RoleManager<RaWMVCRole> roleManager, INotyfService notyf)
        {
            _roleManager = roleManager;
            _notyf = notyf;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel roleVM)
        {
            if (ModelState.IsValid)
            {
                var existingRole = await _roleManager.FindByNameAsync(roleVM.Name);
                if (existingRole != null)
                {
                    _notyf.Warning("Role with this name already exists.");
                    return View(nameof(Index), roleVM);
                }

                if (roleVM.Name.Length > 75)
                {
                    _notyf.Warning("Role name cannot be longer than 75 characters.");

                    return View(nameof(Index), roleVM);
                }

                if (!string.IsNullOrEmpty(roleVM.Description) && roleVM.Description.Length > 200)
                {
                    _notyf.Warning("Role description cannot be longer than 200 characters.");

                    return View(nameof(Index), roleVM);
                }

                var newRole = new RaWMVCRole
                {
                    Name = roleVM.Name,
                    NormalizedName = roleVM.Name.ToUpper(),
                    Description = roleVM.Description,
                };

                var result = await _roleManager.CreateAsync(newRole);
                if (result.Succeeded)
                {
                    _notyf.Success("Role added successfully.");

                    return RedirectToAction("Index");
                }

                // Add errors if creation failed
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(nameof(Index), roleVM);
        }

        // GET: Edit a role (fetch data by role id)
        public async Task<IActionResult> Edit(string id)
        {
            //var role = await _roleManager.FindByIdAsync(id);
            var role = await _roleManager.Roles.Where(r => r.Id == id).SingleOrDefaultAsync();
            if (role == null) return BadRequest();

            var model = new RoleViewModel
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
            };

            return View(nameof(Index), model);
        }

        // POST: Edit a role (submit changes)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleViewModel roleVM)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleVM.Id);
                if (role == null) return BadRequest();

                role.Name = roleVM.Name;
                role.NormalizedName = roleVM.Name?.ToUpper();
                role.Description = roleVM.Description;

                await _roleManager.UpdateAsync(role);

                _notyf.Success("Edited role successfully.");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Failed to edit role";
                _notyf.Error("Failed to edit role");

                return View(nameof(Index), roleVM);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            var status = false;
            var message = "Not yet implemented!!!";
            try
            {
                //=== Predicate/delgate ===//
                var role = await _roleManager.FindByIdAsync(id);
                //=== Remove Role ====//
                var result = await _roleManager.DeleteAsync(role);
                status = true;
                message = "Delete role successfully!!!";
            }
            catch
            {
                message = "Execution error!!!";
            }
            return Json(new { status, message });
        }
        public IActionResult ReloadRoleList()
        {
            return ViewComponent(nameof(RoleList));
        }
    }
}
