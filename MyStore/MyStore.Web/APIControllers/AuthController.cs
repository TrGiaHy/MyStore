using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model;
using Repository.ViewModels;
using static BusinessLogic.Services.ApiClientService.ApiClientService;

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
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Username already exists.",
                        StatusCode = 400
                    });
                }

                // Kiểm tra trùng Email
                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Email already exists.",
                        StatusCode = 400
                    });
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
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description)),
                        StatusCode = 400
                    });
                }

                // Gán role mặc định
                await _userManager.AddToRoleAsync(user, "Customer");

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = "User registered successfully!",
                    StatusCode = 200,
                    Username = user.UserName
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = $"Internal server error: {ex.Message}",
                    StatusCode = 500
                });
            }
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username)
                           ?? await _userManager.FindByEmailAsync(model.Username);

                if (user == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Account does not exist",
                        StatusCode = 400
                    });

                }
                if (!user.IsActive)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Your account has been locked.",
                        StatusCode = 400
                    });

                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Invalid password",
                        StatusCode = 400
                    });

                }
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = "Login Successfully!",
                    StatusCode = 200
                });

            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = $"Internal server error: {ex.Message}",
                    StatusCode = 500
                });
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
