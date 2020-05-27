using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace DatingApp.API.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        // private readonly IAuthRepository _repo;
        // IAuthRepository repo, 
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly  SignInManager<User> _signInManager;

        public AuthController(IConfiguration config, IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            // _repo = repo;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            // userForRegisterDto.UserName = userForRegisterDto.UserName.ToLower();

            // if (await _repo.UserExists(userForRegisterDto.UserName))
            //     return BadRequest("Username already exists");

            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);

            // var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            var userToReturn = _mapper.Map<UserForListDto>(userToCreate);

            if (result.Succeeded)
            {
                return CreatedAtRoute("GetUser", new { Controller = "Users", id = userToCreate.Id }, userToReturn);
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLogin userForLogin)
        {
            // var userFromRepo = await _repo.Login(userForLogin.UserName.ToLower(), userForLogin.Password);
            // if (userFromRepo == null)
            //     return Unauthorized();

            var user = await _userManager.FindByNameAsync(userForLogin.UserName);

            if (user == null)
                return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLogin.Password, false);

            if (result.Succeeded)
            {
                // Authenticated - create claims
                var appUser = _mapper.Map<UserForListDto>(user);

                // Return the token
                return Ok(new
                {
                    token = await GenerateJwtToken(user), // .Result,  // tokenHandler.WriteToken(token),
                    user = appUser
                });
            }

            return Unauthorized();
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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

            return tokenHandler.WriteToken(token);
        }
    }
}