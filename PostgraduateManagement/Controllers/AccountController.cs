// File: Controllers/AccountController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PostgraduateManagement.ViewModels;

namespace PostgraduateManagement.Controllers
{
    /// <summary>
    /// Controller quản lý xác thực và phân quyền tài khoản người dùng theo vai trò
    /// (Học viên, Giảng viên, Hội đồng sau đại học, Quản trị hệ thống).
    /// </summary>
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Giao diện đăng nhập hệ thống.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Thesis");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        /// <summary>
        /// Xử lý đăng nhập hệ thống với ASP.NET Core Identity.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Người dùng {Username} đăng nhập thành công.", model.Username);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Thesis");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Tài khoản người dùng {Username} đã bị tạm khóa do nhập sai nhiều lần.", model.Username);
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị tạm khóa do nhập sai mật khẩu nhiều lần. Vui lòng thử lại sau.");
                return View(model);
            }

            _logger.LogWarning("Đăng nhập không thành công cho tài khoản: {Username}", model.Username);
            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            return View(model);
        }

        /// <summary>
        /// Đăng xuất khỏi hệ thống.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Người dùng đã đăng xuất khỏi hệ thống.");
            return RedirectToAction("Index", "Thesis");
        }

        /// <summary>
        /// Trang thông báo từ chối quyền truy cập (Access Denied).
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Giao diện đăng ký tài khoản mới cho người dùng.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        /// <summary>
        /// Xử lý đăng ký tài khoản mới kèm phân quyền vai trò.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Username.Trim(),
                Email = model.Email.Trim()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Đăng ký thành công tài khoản người dùng: {Username}", user.UserName);
                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công. Vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
