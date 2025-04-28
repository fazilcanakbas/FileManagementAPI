using FileManagementAPI.DTOs;
using FileManagementAPI.Models;
using FileManagementAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FileManagementAPI.Controllers
{
    public class AuthController : BaseController
    {
        private readonly UserRepository _userRepository;
        private readonly JwtConfig _jwtConfig;

        public AuthController(UserRepository userRepository, IOptions<JwtConfig> jwtConfig)
        {
            _userRepository = userRepository;
            _jwtConfig = jwtConfig.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingUser = await _userRepository.GetUserByEmailAsync(registerDto.Email);
                if (existingUser != null)
                    return BadRequest(new { Error = "User with this email already exists." });

                var existingUsername = await _userRepository.GetUserByUsernameAsync(registerDto.Username);
                if (existingUsername != null)
                    return BadRequest(new { Error = "User with this username already exists." });

                var user = new AppUser
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName
                };

                var result = await _userRepository.CreateUserAsync(user, registerDto.Password);
                if (!result.Succeeded)
                    return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

                await _userRepository.AddToRoleAsync(user, "User");

                return Ok(new { Message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _userRepository.GetUserByUsernameAsync(loginDto.Username);
                if (user == null)
                    return Unauthorized(new { Error = "Invalid username or password." });

                var passwordValid = await _userRepository.CheckPasswordAsync(user, loginDto.Password);
                if (!passwordValid)
                    return Unauthorized(new { Error = "Invalid username or password." });

                var token = await GenerateJwtToken(user);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        private async Task<TokenDto> GenerateJwtToken(AppUser user)
        {
            var roles = await _userRepository.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new TokenDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expires,
                Username = user.UserName ?? string.Empty,
                Roles = roles.ToList()
            };
        }
    }
}