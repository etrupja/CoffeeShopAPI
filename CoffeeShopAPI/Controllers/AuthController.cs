using CoffeeShopAPI.Data;
using CoffeeShopAPI.Data.DTOs;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoffeeShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]

    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> Register([FromBody] RegisterDto payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all required fields");
            }

            var userExists = await _userManager.FindByEmailAsync(payload.Email);

            if (userExists != null)
            {
                return BadRequest($"User {payload.Email} already exists");
            }

            IdentityUser newUser = new IdentityUser()
            {
                Email = payload.Email,
                UserName = payload.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(newUser, payload.Password);

            if (!result.Succeeded)
            {
                return BadRequest("User could not be created!");
            }

            //await _userManager.AddToRoleAsync(newUser, "User");

            return Created(nameof(Register), $"User {payload.Email} created");
        }

        [HttpPost("login-user")]
        public async Task<IActionResult> Login([FromBody] LoginDto payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all required fields");
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, payload.Password))
            {
                var tokenValue = await GenerateJwtToken(user);

                return Ok(tokenValue);
            }

            return Unauthorized();
        }



        private async Task<AuthResultDto> GenerateJwtToken(IdentityUser user)
        {
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            //Add User Roles
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }


            var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("this-is-just-a-secret-key-here-stored-somewhere-else"));

            var token = new JwtSecurityToken(
                issuer: "https://localhost:44382/",
                audience: "https://localhost:44382/",
                expires: DateTime.UtcNow.AddMinutes(10), // 5 - 10mins
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();

            var response = new AuthResultDto()
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                ExpiresAt = token.ValidTo
            };

            return response;
        }
    }
}
