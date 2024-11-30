// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using RaWMVC.Areas.Identity.Data;
using RaWMVC.Commons;

namespace RaWMVC.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<RaWMVCUser> _userManager;
        private readonly SignInManager<RaWMVCUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private readonly INotyfService _notyf;

        public IndexModel(
            UserManager<RaWMVCUser> userManager,
            SignInManager<RaWMVCUser> signInManager,
            IWebHostEnvironment environment, INotyfService notyf)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
            _notyf = notyf;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "First Name")]
            public string FirstName { get; set; }
            [Display(Name = "Last Name")]
            public string LastName { get; set; }
            [Display(Name = "Email")]
            public string? Email { get; set; }
            public string? Phonenumber { get; set; }
            [Display(Name = "Username")]
            public string Username { get; set; }
            [Display(Name = "Introduction")]
            public string Introduction { get; set; }
            public string ProfilePicture { get; set; }
            [BindProperty]
            public IFormFile? ProfilePictureFile { get; set; }
            public DateOnly? DateOfBirth { get; set; }
        }

        private async Task LoadAsync(RaWMVCUser user)
        {
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var userName = await _userManager.GetUserNameAsync(user);
            var firstName = user.FirstName;
            var lastName = user.LastName;
            var introduction = user.Introduction;
            var profilePicture = user.ProfilePicture;
            

            Username = userName;

            Input = new InputModel
            {
                Phonenumber = phoneNumber,
                Username = userName,
                FirstName = firstName,
                LastName = lastName,
                Introduction = introduction,
                ProfilePicture = profilePicture
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
            }

            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
            }

            if (Input.Introduction != user.Introduction)
            {
                user.Introduction = Input.Introduction;
            }

            if (!string.IsNullOrEmpty(Input.Phonenumber) && Input.Phonenumber != user.PhoneNumber)
            {
                user.PhoneNumber = Input.Phonenumber;
            }

            //=== Handling avatar updates ===//
            if (Request.Form.Files.Count > 0)
            {
                IFormFile file = Request.Form.Files.FirstOrDefault();
                if (file != null)
                {
                    var profilePicturePath = await SaveProfilePicture(file);
                    if (!string.IsNullOrEmpty(profilePicturePath))
                    {
                        user.ProfilePicture = profilePicturePath;
                    }
                }
            }

            //=== Update all data fields at once ===//
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await LoadAsync(user);
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _notyf.Success("Your profile has been updated");
            return RedirectToPage();
        }


        private async Task<string> SaveProfilePicture(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "You must add a profile picture file.";
                return null;
            }

            var fileGuidName = Guid.NewGuid().ToString();
            var fileExtension = Path.GetExtension(file.FileName);

            if (string.IsNullOrEmpty(fileExtension) || !Constants.Valid_Extenstion.Contains(fileExtension.TrimStart('.')))
            {
                TempData["Message"] = "Invalid file extension.";
                return null;
            }

            var uniqueFileName = $"{fileGuidName}{fileExtension}";
            var avatarFolder = Path.Combine(_environment.WebRootPath, "avatar");

            if (!Directory.Exists(avatarFolder))
            {
                Directory.CreateDirectory(avatarFolder);
            }

            var filePath = Path.Combine(avatarFolder, uniqueFileName);
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return $"/avatar/{uniqueFileName}";
            }
            catch
            {
                TempData["Message"] = "An error occurred while saving the profile picture.";
                return null;
            }
        }


    }
}