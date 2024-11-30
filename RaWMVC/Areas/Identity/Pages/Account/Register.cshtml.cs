// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using AspNetCore;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Data;
using RaWMVC.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace RaWMVC.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<RaWMVCUser> _signInManager;
        private readonly UserManager<RaWMVCUser> _userManager;
        private readonly IUserStore<RaWMVCUser> _userStore;
        private readonly IUserEmailStore<RaWMVCUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RaWDbContext _context;
        private readonly INotyfService _notyf;

        public RegisterModel(
            UserManager<RaWMVCUser> userManager,
            IUserStore<RaWMVCUser> userStore,
            SignInManager<RaWMVCUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RaWDbContext context, INotyfService notyf)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _notyf = notyf;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            //=== Như thêm vào ===//
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "User Name")]
            [StringLength(256, ErrorMessage = "The username max {2} and {1}characters long.", MinimumLength = 10)]
            public string UserName { get; set; }
            //[Required]
            //         [EmailAddress]
            //         [Display(Name = "Email")]
            //         public string Email { get; set; }

            [Required]
            [StringLength(30, ErrorMessage = "The password must be between {2} and {1} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$", 
                ErrorMessage = "The password must contain at least one uppercase letter, one digit, and one special character.")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
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
            //returnUrl ??= Url.Content("~/");
            returnUrl ??= Url.Content("~/Identity/Account/Login");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.JoinedDate = DateTime.UtcNow;

                //=== Tạo tài khoản bằng username ===//
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

                    await _userManager.UpdateAsync(user);

                    //=== Create Library entry for the user ===//
                    var library = new Library
                    {
                        Id = Guid.NewGuid().ToString(),
                        StoryId = null,
                        UserId = user.Id,
                        InMyLibrary = true,
                    };

                    await _context.Libraries.AddAsync(library);
                    await _context.SaveChangesAsync();

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
