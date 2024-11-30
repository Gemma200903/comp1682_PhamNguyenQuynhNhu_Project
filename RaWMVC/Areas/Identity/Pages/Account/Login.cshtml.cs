// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RaWMVC.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace RaWMVC.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<RaWMVCUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<RaWMVCUser> _userManager;

        public LoginModel(SignInManager<RaWMVCUser> signInManager, ILogger<LoginModel> logger, UserManager<RaWMVCUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "User Name")]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Home/Index");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Find the user with case-sensitive username comparison
                var user = _userManager.Users
                    .AsEnumerable()
                    .FirstOrDefault(u => string.Equals(u.UserName, Input.UserName, StringComparison.Ordinal));

                if (user == null)
                {
                    // Username not found
                    ModelState.AddModelError(string.Empty, "Username or password is not correct.");
                    return Page();
                }

                // Validate password
                var passwordValid = await _userManager.CheckPasswordAsync(user, Input.Password);

                if (passwordValid)
                {
                    // Correct password, sign in
                    await _signInManager.SignInAsync(user, Input.RememberMe);
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    // Incorrect password
                    ModelState.AddModelError(string.Empty, "Username or password is not correct.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay the form
            return Page();
        }
    }
}