using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Data;
using RaWMVC.ViewComponents;
using RaWMVC.ViewModels;

namespace RaWMVC.Controllers
{
    //[Authorize(Roles = "Admintrator")]
    public class UserController : Controller
    {
        private readonly RaWDbContext _context;
        private readonly SignInManager<RaWMVCUser> _signInManager;
        private readonly UserManager<RaWMVCUser> _userManager;
        private readonly RoleManager<RaWMVCRole> _roleManager;
        private readonly ILogger<UserController> _logger;
        private readonly INotyfService _notyf;

        public UserController(RaWDbContext context, UserManager<RaWMVCUser> userManager,
            SignInManager<RaWMVCUser> signInManager, RoleManager<RaWMVCRole> roleManager,
            ILogger<UserController> logger, INotyfService notyf)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Roles = await GetSelectListRole(null);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountViewModel accountVM)
        {
            // Kiểm tra nếu Username hoặc Password để trống
            if (string.IsNullOrWhiteSpace(accountVM.Username))
            {
                _notyf.Error("Username is required.");
            }

            if (string.IsNullOrWhiteSpace(accountVM.Password))
            {
                _notyf.Error("Password is required.");
            }

            // Kiểm tra độ dài của Username và Password
            if (accountVM.Username?.Length > 256)
            {
                _notyf.Error("Username cannot exceed 256 characters.");
            }

            if (accountVM.Password?.Length > 30)
            {
                _notyf.Error("Password cannot exceed 30 characters.");
            }

            // Nếu có lỗi, trả về View để người dùng chỉnh sửa
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await GetSelectListRole(accountVM.RoleId);
                return View(nameof(Index), accountVM);
            }

            var newUser = new RaWMVCUser
            {
                UserName = accountVM.Username
            };

            var result = await _userManager.CreateAsync(newUser, accountVM.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account.");

                if (!string.IsNullOrEmpty(accountVM.RoleId))
                {
                    var role = await _roleManager.FindByIdAsync(accountVM.RoleId);
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(newUser, role.Name);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Role does not exist.");
                        ViewBag.Roles = await GetSelectListRole(accountVM.RoleId);
                        return View(nameof(Index), accountVM);
                    }
                }

                await _signInManager.SignInAsync(newUser, isPersistent: false);
                _notyf.Success("Account created successfully with role.");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Xử lý khi không tạo được tài khoản
                _logger.LogWarning("Failed to create user account.");
                _notyf.Error("Failed to create account. Please try again.");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewBag.Roles = await GetSelectListRole(accountVM.RoleId);
            return View(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.Users.Where(r => r.Id == id).SingleOrDefaultAsync();
            if (user == null) return BadRequest();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentRole = currentRoles.FirstOrDefault();

            ViewBag.Roles = await GetSelectListRole(null);

            var model = new AccountViewModel
            {
                Id = user.Id,
                Username = user.UserName, // Change Email to Username
                RoleName = currentRole
            };

            return View(nameof(Index), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AccountViewModel accountVM)
        {
            try
            {
                var account = await _userManager.FindByIdAsync(accountVM.Id);
                if (account == null) return BadRequest("User not found");

                // Cập nhật username
                account.UserName = accountVM.Username;

                var updateResult = await _userManager.UpdateAsync(account);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    _notyf.Error("Account updated failed!");

                    ViewBag.Roles = await GetSelectListRole(accountVM.RoleId);
                    return View("Index", accountVM); // Trả về form Index
                }

                // Xử lý vai trò
                var currentRoles = await _userManager.GetRolesAsync(account);

                if (currentRoles.Any())
                {
                    ViewBag.Roles = await GetSelectListRole(null);

                    await _userManager.RemoveFromRolesAsync(account, currentRoles);
                }

                if (!string.IsNullOrEmpty(accountVM.RoleId))
                {
                    var newRole = await _roleManager.FindByIdAsync(accountVM.RoleId);
                    if (newRole != null)
                    {
                        ViewBag.Roles = await GetSelectListRole(null);

                        await _userManager.AddToRoleAsync(account, newRole.Name);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Selected role does not exist.");
                        ViewBag.Roles = await GetSelectListRole(accountVM.RoleId);
                        return View("Index", accountVM);
                    }
                }

                _notyf.Success("Account updated successfully.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyf.Error($"Failed to edit account. Error: {ex.Message}");

                ViewBag.Roles = await GetSelectListRole(accountVM.RoleId);
                return View("Index", accountVM);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var status = false;
            var message = "Not yet implemented!";
            try
            {
                var account = await _userManager.FindByIdAsync(id);
                if (account == null)
                {
                    message = "Account not found!";
                    return Json(new { status, message });
                }

                var result = await _userManager.DeleteAsync(account);
                if (result.Succeeded)
                {
                    status = true;
                    message = "Account deleted successfully.";
                }
                else
                {
                    message = "Error deleting account.";
                }
            }
            catch (Exception ex)
            {
                message = "Execution error: " + ex.Message;
            }
            return Json(new { status, message });
        }

        public IActionResult ReloadAccountList()
        {
            return ViewComponent(nameof(AccountList));
        }

        public async Task<SelectList> GetSelectListRole(string? idChosen)
        {
            var listRole = await _roleManager.Roles
                .OrderBy(c => c.Position)
                .AsNoTracking()
                .Select(c => new RoleViewModel
                {
                    Id = c.Id,
                    Name = $"{c.Position}. {c.Name}"
                })
                .ToListAsync();

            listRole.Insert(0, new RoleViewModel { Id = "", Name = "=== Choose Role ===" });

            var selectedItem = !string.IsNullOrEmpty(idChosen) ? idChosen : listRole[0].Id;

            var selectList = new SelectList(listRole, "Id", "Name", selectedItem);
            return selectList;
        }
    }
}
