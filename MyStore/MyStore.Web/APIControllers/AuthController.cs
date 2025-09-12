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

        //[HttpPost("Login")]
        //public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        //{
        //    try
        //    {
        //        var user = await _userManager.FindByNameAsync(model.Username)
        //                   ?? await _userManager.FindByEmailAsync(model.Username);

        //        if (user == null)
        //        {
        //            return BadRequest(new { message = "Account does not exist" });
        //        }
        //        if (!user.IsActive)
        //        {
        //            return BadRequest(new { message = "Your account has been locked." });
        //        }

        //        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        //        if (!result.Succeeded)
        //        {
        //            return Unauthorized(new { message = "Invalid password" });
        //        }

        //        // ✅ Tạo JWT token
        //        var authClaims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.NameIdentifier, user.Id),
        //    new Claim(ClaimTypes.Name, user.UserName),
        //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //};

        //        var authSigningKey = new SymmetricSecurityKey(
        //            Encoding.UTF8.GetBytes("MySuperUltraStrongSecretKey1234567890!!")
        //        );

        //        var token = new JwtSecurityToken(
        //            issuer: "MyStore",
        //            audience: "MyStoreClient",
        //            expires: DateTime.UtcNow.AddHours(1),
        //            claims: authClaims,
        //            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        //        );

        //        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        //        // ✅ Trả về chuẩn OAuth style
        //        return Ok(new
        //        {
        //            token = tokenString,
        //            token_type = "Bearer",
        //            expires_in = 3600, // giây (1 giờ)
        //            expiration = token.ValidTo
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
        //    }
        //}
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username)
                           ?? await _userManager.FindByEmailAsync(model.Username);

                if (user == null)
                {
                    return BadRequest(new { message = "Account does not exist" });
                }
                if (!user.IsActive)
                {
                    return BadRequest(new { message = "Your account has been locked." });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized(new { message = "Invalid password" });
                }

                return Ok(new { message = "Login Success" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal Server Error: {ex.Message}" });
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
