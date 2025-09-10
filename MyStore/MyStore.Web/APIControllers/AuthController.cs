using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model;
using Repository.ViewModels;

namespace MyStore.Web.APIControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Đăng ký
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            try
            {
                // Kiểm tra trùng Username
                var existingUserByName = await _userManager.FindByNameAsync(model.Username);
                if (existingUserByName != null)
                {
                    return BadRequest("Username already exists.");
                }

                // Kiểm tra trùng Email
                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                {
                    return BadRequest("Email already exists.");
                }

                var user = new AppUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = model.Address,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors.Select(e => e.Description));
                }

                // Gán role mặc định "Customer"
                await _userManager.AddToRoleAsync(user, "Customer");

                return Ok("User registered successfully!");
            }
            catch (Exception ex)
            {
                // Log lỗi (nếu có ILogger thì log lại)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username) ?? await _userManager.FindByEmailAsync(model.Username);
                if (user == null)
                {
                    return BadRequest("Account does not exist");
                }
                if (!user.IsActive)
                    return BadRequest("Your account has been locked.");

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded)
                    return Unauthorized("Invalid Password");

                return Ok("Login successful!");
            }
            catch (Exception ex)
            {
                return BadRequest("Unknown error, please try again.");
            }
        }

        //[HttpPost("Logout")]
        //public async Task<IActionResult> Logout()
        //{
        //    try
        //    {
        //        if (!User.Identity?.IsAuthenticated ?? false)
        //        {
        //            return BadRequest("You are not logged in.");
        //        }
        //        await _signInManager.SignOutAsync();
        //        return Ok("Logout successful!");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}
    }
}
