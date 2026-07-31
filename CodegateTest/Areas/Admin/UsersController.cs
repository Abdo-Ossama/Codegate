using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CodegateTest.Areas.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area(SD.ADMIN_ROLE)]
    [Authorize(Roles = SD.ADMIN_ROLE)]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        [HttpGet("")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> GetAll(string? search, int page = 1)
        {
            var users = _userManager.Users.AsNoTracking();

            var totalUsers = await users.CountAsync();

            if (page <= 1)
            {
                page = 1;
            }

            int pageSize = 5;
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var finalUsers = await users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<object>();

            foreach (var user in finalUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.EmailConfirmed,
                    Role = roles.FirstOrDefault() 
                });
            }

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalUsers = totalUsers,
                Users = result
            });
        }

        
        [HttpGet("{id}")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Get(string id)
        {

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return BadRequest(new APIResponce()
                {
                    Message = ["User is not Found"]
                });
            }
            var role = await _userManager.GetRolesAsync(user);
            return Ok(new
            {
                user.UserName,
                user.Email,
                user.EmailConfirmed,
                role 
            });
        }

        [HttpPost]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Create(CreateUserRequest createUserRequest)
        {
            var user = createUserRequest.Adapt<ApplicationUser>();

            user.UserName = createUserRequest.Email;
            user.EmailConfirmed = true;

            var result = await _userManager.CreateAsync(
                user,
                createUserRequest.Password
            );

            if (!result.Succeeded)
            {
                return BadRequest(new APIResponce
                {
                    Message = ["Failed to create user"]

                });
            }

            var roleResult = await _userManager.AddToRoleAsync(
                user,
                createUserRequest.Role
            );

            if (!roleResult.Succeeded)
            {
                return BadRequest(new APIResponce
                {
                    Message = ["Failed to add role to the user"]

                });
            }

            return Ok(new
            {
                Message = "User created successfully",
                UserId = user.Id
            });
        }




        [HttpPut("{id}")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Update(
     string id,
     UpdateUserRequest updateUserRequest)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
            {
                return BadRequest(new APIResponce()
                {
                    Message = ["User is not Found"]
                });
            }

            user.Fname = updateUserRequest.FirstName;
            user.Lname = updateUserRequest.LastName;

            var usernameResult = await _userManager.SetUserNameAsync(
                user,
                updateUserRequest.Email
            );

            if (!usernameResult.Succeeded)
            {
                return BadRequest(new APIResponce()
                {
                    Message = ["Failed to update username"]
                });
            }

            var emailResult = await _userManager.SetEmailAsync(
                user,
                updateUserRequest.Email
            );

            if (!emailResult.Succeeded)
            {
                return BadRequest(new APIResponce()
                {
                    Message = ["Failed to update email"]
                });
            }

            if (!string.IsNullOrWhiteSpace(updateUserRequest.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var passwordResult = await _userManager.ResetPasswordAsync(
                    user,
                    token,
                    updateUserRequest.Password
                );

                if (!passwordResult.Succeeded)
                {
                    return BadRequest(new APIResponce()
                    {
                        Message = ["Failed to update password"]
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(updateUserRequest.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);

                if (currentRoles.Any())
                {
                    var removeRoleResult =
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!removeRoleResult.Succeeded)
                    {
                        return BadRequest(new APIResponce()
                        {
                            Message = ["Failed to remove old role"]
                        });
                    }
                }

                var addRoleResult = await _userManager.AddToRoleAsync(
                    user,
                    updateUserRequest.Role
                );

                if (!addRoleResult.Succeeded)
                {
                    return BadRequest(new APIResponce()
                    {
                        Message = ["Failed to add new role"]
                    });
                }
            }

            return Ok(new
            {
                Message = "User updated successfully",
                UserId = user.Id
            });
        }



        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
            {
                return BadRequest(new APIResponce()
                {
                    Message = ["User is not Found"]
                });
            }

         
            if (user.LockoutEnd.HasValue &&
                user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                var result = await _userManager.SetLockoutEndDateAsync(
                    user,
                    null
                );

                if (!result.Succeeded)
                {
                    return BadRequest(new APIResponce()
                    {
                        Message = ["Failed to activate user"]
                    });
                }

                return Ok(new
                {
                    Message = "User activated successfully",
                    IsLocked = false
                });
            }

        
            var lockResult = await _userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.UtcNow.AddYears(100)
            );

            if (!lockResult.Succeeded)
            {
                return BadRequest(new APIResponce()
                {
                    Message = ["Failed to lock user"]
                });
            }

            return Ok(new
            {
                Message = "User locked successfully",
                IsLocked = true
            });
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = SD.ADMIN_ROLE)]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
            {
                return BadRequest(new APIResponce()
                {
                    Message = ["User is not Found"]
                });
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new APIResponce()
                {
                    Message = ["Failed to delete user"]
                });
            }

            return Ok(new
            {
                Message = "User deleted successfully",
                UserId = user.Id
            });
        }

    }
}
