using System.Security.Claims;
using BusinessLogic.Services.ApiClientService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model;
using Repository.ViewModels;

namespace MyStore.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IApiClientService _apiClient;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;


        public AuthenticationController(IApiClientService apiClient, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _apiClient = apiClient;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            //if (User.Identity.IsAuthenticated)
            //{
            //    return RedirectToAction("Index", "Home");
            //}

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            // 1. Kiểm tra validation phía server
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                return Json(new { success = false, message = firstError ?? "Invalid data" });
            }

            try
            {
                // Gọi API đăng nhập
                var apiResponse = await _apiClient.PostJsonAsync<object>("/api/Auth/Login", model);

                var user = await _userManager.FindByNameAsync(model.Username)
                           ?? await _userManager.FindByEmailAsync(model.Username);

                if (apiResponse != null && apiResponse.Success)
                {
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    // Đăng nhập thành công
                    return Json(new
                    {
                        success = true,
                        message = "Đăng nhập thành công",
                        redirectUrl = Url.Action("Index", "Customer")
                    });
                }

                // Nếu thất bại, trả về message từ API
                return Json(new
                {
                    success = false,
                    message = apiResponse?.ErrorMessage ?? "Đăng nhập thất bại: Sai tài khoản hoặc mật khẩu."
                });
            }
            catch (Exception)
            {
                // Chỉ show lỗi hệ thống thực sự
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."
                });
            }
        }

        public IActionResult CheckLogin()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return Content($"User đã đăng nhập: {User.Identity.Name}");
            return Content("Chưa đăng nhập hoặc cookie không tồn tại.");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                return Json(new { success = false, message = firstError ?? "Invalid data" });
            }

            try
            {
                var apiResponse = await _apiClient.PostJsonAsync<string>("/api/Auth/Register", model);

                if (apiResponse != null && apiResponse.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = apiResponse.Data,
                        redirectUrl = Url.Action("Login", "Authentication")
                    });
                }

                return Json(new
                {
                    success = false,
                    message = apiResponse?.ErrorMessage ?? "Đăng ký thất bại."
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."
                });
            }
        }
        public async Task<IActionResult> Logout()
        {

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Customer");
        }
    }
}
