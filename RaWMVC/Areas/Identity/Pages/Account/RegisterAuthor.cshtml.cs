// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Data;
using System.ComponentModel.DataAnnotations;

namespace RaWMVC.Areas.Identity.Pages.Account
{
    public class RegisterAuthorModel : PageModel
    {
        private readonly SignInManager<RaWMVCUser> _signInManager;
        private readonly UserManager<RaWMVCUser> _userManager;
        private readonly IUserStore<RaWMVCUser> _userStore;
        private readonly IUserEmailStore<RaWMVCUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RaWDbContext _context;
        private readonly RoleManager<RaWMVCRole> _roleManager;
        private readonly INotyfService _notyf;

        public RegisterAuthorModel(
            UserManager<RaWMVCUser> userManager,
            IUserStore<RaWMVCUser> userStore,
            SignInManager<RaWMVCUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RaWDbContext context,
            RoleManager<RaWMVCRole> roleManager, INotyfService notyf)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _roleManager = roleManager;
            _notyf = notyf;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "User Name")]
            public string UserName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            [RegularExpression(@"^(?=.*[A-Z])(?=.*\W).{8,}$", ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, and one special character.")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime JoinedDate { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Identity/Account/Login");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByNameAsync(Input.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Input.UserName", "Username is already taken. Please choose a different one.");
                    return Page();
                }

                var user = CreateUser();
                user.JoinedDate = DateTime.UtcNow;

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);

                var userNameExists = await _userManager.FindByNameAsync(Input.UserName);
                if (userNameExists != null)
                {
                    _notyf.Error("User name already taken. Select a different username.");
                    return RedirectToPage();
                }

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var roleExists = await _roleManager.RoleExistsAsync("Author");
                    if (!roleExists)
                    {
                        var role = new RaWMVCRole {
                            Name = "Author",
                        };
                        await _roleManager.CreateAsync(role);
                    }

                    await _userManager.AddToRoleAsync(user, "Author");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _notyf.Success("Successfully created account");
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private RaWMVCUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<RaWMVCUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(RaWMVCUser)}'. " +
                    $"Ensure that '{nameof(RaWMVCUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<RaWMVCUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<RaWMVCUser>)_userStore;
        }
    }
}