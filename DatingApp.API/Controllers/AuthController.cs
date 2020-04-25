using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegister userForRegister)
        {
            userForRegister.UserName = userForRegister.UserName.ToLower();

            if (await _repo.UserExists(userForRegister.UserName))
                return BadRequest("Username already exists");

            var userToCreate = new User() 
            {
                UserName = userForRegister.UserName
            };

            var createdUser = await _repo.Register(userToCreate, userForRegister.Password);

            // Fix this when we can get the user (201 = created at route)
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLogin userForLogin)
        {
            var userFromRepo = await _repo.Login(userForLogin.UserName.ToLower(), userForLogin.Password);

            if (userFromRepo == null)
                return Unauthorized();

            // Authenticated - create claims
            var claims = new[] 
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.UserName)
            };

            // Create the signing key
            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            // Use the key
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Create the token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            // Create the token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // Bring it all together
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Return the token
            return Ok(new 
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}