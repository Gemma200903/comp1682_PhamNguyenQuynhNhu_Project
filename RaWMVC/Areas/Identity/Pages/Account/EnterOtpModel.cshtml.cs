using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using RaWMVC.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace RaWMVC.Areas.Identity.Pages.Account
{
    public class EnterOtpModel : PageModel
    {
        private readonly UserManager<RaWMVCUser> _userManager;
        private readonly ILogger<EnterOtpModel> _logger;

        public EnterOtpModel(UserManager<RaWMVCUser> userManager, ILogger<EnterOtpModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // Định nghĩa InputModel với các thuộc tính cần thiết
        public class InputModel
        {
            public string Otp { get; set; }
            public string PhoneNumber { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }  // Đảm bảo Input được khai báo ở đây

        // Phương thức GET để hiển thị trang và truyền PhoneNumber
        public IActionResult OnGet(string phoneNumber)
        {
            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
            return Page();
        }

        // Phương thức POST để kiểm tra OTP
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Input.Otp))
            {
                ModelState.AddModelError(string.Empty, "Please enter the OTP.");
                return Page();
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == Input.PhoneNumber);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            //if (user.ResetPasswordOtp != Input.Otp || user.ResetPasswordOtpExpiry < DateTime.UtcNow)
            //{
            //    ModelState.AddModelError(string.Empty, "Invalid or expired OTP.");
            //    return Page();
            //}

            return RedirectToPage("./ResetPassword", new { phoneNumber = Input.PhoneNumber });
        }
    }
}