using FileManagementAPI.DTOs;
using FileManagementAPI.Models;
using FileManagementAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileManagementAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        private readonly UserRepository _userRepository;

        public UsersController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var users = await _userRepository.GetPagedUsersAsync(page, pageSize);
                var totalCount = await _userRepository.CountUsersAsync();

                var userDtos = new List<UserDto>();
                foreach (var user in users)
                {
                    var roles = await _userRepository.GetRolesAsync(user);
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        Username = user.UserName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CreatedAt = user.CreatedAt,
                        Roles = roles.ToList()
                    });
                }

                return Ok(new UserListDto
                {
                    Users = userDtos,
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    CurrentPage = page
                });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { Error = "User not found" });

                var roles = await _userRepository.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPost("{id}/role")]
        public async Task<IActionResult> AddUserToRole(string id, [FromBody] string role)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { Error = "User not found" });

                if (string.IsNullOrWhiteSpace(role))
                    return BadRequest(new { Error = "Role name is required" });

                if (role != "Admin" && role != "User")
                    return BadRequest(new { Error = "Invalid role name. Must be 'Admin' or 'User'" });

                if (await _userRepository.IsInRoleAsync(user, role))
                    return BadRequest(new { Error = "User already has this role" });

                var result = await _userRepository.AddToRoleAsync(user, role);
                if (!result.Succeeded)
                    return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

                return Ok(new { Message = $"User added to role '{role}' successfully" });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDto userDto)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { Error = "User not found" });

                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userRepository.UpdateUserAsync(user);
                if (!result.Succeeded)
                    return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

                return Ok(new { Message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { Error = "User not found" });

                var result = await _userRepository.DeleteUserAsync(user);
                if (!result.Succeeded)
                    return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

                return Ok(new { Message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
    }
}