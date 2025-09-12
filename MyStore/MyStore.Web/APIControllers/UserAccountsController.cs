using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model;
using Repository.ViewModels;

namespace MyStore.Web.APIControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccountsController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public UserAccountsController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // lấy danh sách user
        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            var users = _userManager.Users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.FullName,
                u.Address,
                u.IsActive
            }).ToList();

            return Ok(users);
        }

        [HttpPut("UpdateUser/{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound("User not found.");

                user.FullName = model.FullName;
                user.Address = model.Address;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return BadRequest(result.Errors.Select(e => e.Description));

                return Ok(new
                {
                    Message = "User updated successfully.",
                    UserId = user.Id,
                    FullName = user.FullName,
                    Address = user.Address
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // HideUser
        [HttpPatch("HideUser/{userId}")]
        public async Task<IActionResult> HideUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound("User not found.");

                user.IsActive = false;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return BadRequest(result.Errors.Select(e => e.Description));

                return Ok(new { Message = "User IsActive updated successfully.", UserId = user.Id, IsActive = user.IsActive });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // HideUser
        [HttpPatch("ShowUser/{userId}")]
        public async Task<IActionResult> ShowUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound("User not found.");

                user.IsActive = true;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return BadRequest(result.Errors.Select(e => e.Description));

                return Ok(new { Message = "User IsActive updated successfully.", UserId = user.Id, IsActive = user.IsActive });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
