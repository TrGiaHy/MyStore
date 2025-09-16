using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model;
using Repository.ViewModels;
using static BusinessLogic.Services.ApiClientService.ApiClientService;

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
            try
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

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = users,
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

        [HttpPut("UpdateUser/{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserViewModel model)
        {
            if (model == null)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = "Invalid data.",
                    StatusCode = 400
                });
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "User not found.",
                        StatusCode = 404
                    });
                }

                user.FullName = model.FullName;
                user.Address = model.Address;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description)),
                        StatusCode = 400
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        Message = "User updated successfully.",
                        UserId = user.Id,
                        FullName = user.FullName,
                        Address = user.Address
                    },
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

        [HttpPatch("HideUser/{userId}")]
        public async Task<IActionResult> HideUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "User not found.",
                        StatusCode = 404
                    });
                }

                user.IsActive = false;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description)),
                        StatusCode = 400
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        Message = "User IsActive updated successfully.",
                        UserId = user.Id,
                        IsActive = user.IsActive
                    },
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

        [HttpPatch("ShowUser/{userId}")]
        public async Task<IActionResult> ShowUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "User not found.",
                        StatusCode = 404
                    });
                }

                user.IsActive = true;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description)),
                        StatusCode = 400
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        Message = "User IsActive updated successfully.",
                        UserId = user.Id,
                        IsActive = user.IsActive
                    },
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

    }
}
