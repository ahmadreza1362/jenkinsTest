using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Datingapp.API.Data;
using Datingapp.API.Dtos;
using Datingapp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Datingapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly SignInManager<User> _singInManager;
        private readonly UserManager<User> _userManager;

        public AuthController(IAuthRepository repo, IConfiguration config,
            IMapper mapper, UserManager<User> userManager, SignInManager<User> singInManager)
        {
            _repo = repo;
            _config = config;
            _mapper = mapper;
            _singInManager = singInManager;
            _userManager = userManager;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //userForRegisterDto.UserName = userForRegisterDto.UserName.ToLower();
            //if (await _repo.UserExists(userForRegisterDto.UserName))
            //    return BadRequest("UserName already exists");

            var userTocreate = _mapper.Map<User>(userForRegisterDto);
            var result = await _userManager.CreateAsync(userTocreate, userForRegisterDto.Password);


            //var createdUser = await _repo.Register(userTocreate, userForRegisterDto.Password);

            var userForReturn = _mapper.Map<UserForDetailsDto>(userTocreate);
            if(result.Succeeded)
            {
                return CreatedAtRoute("GetUser", new { controller = "Users", id = userTocreate.Id }, userForReturn);
            }

            return BadRequest(result.Errors);


        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLogin)
        {

            var user = await _userManager.FindByNameAsync(userForLogin.UserName);    //await _repo.Login(userForLogin.UserName, userForLogin.Password);
            var result = await _singInManager.CheckPasswordSignInAsync(user, userForLogin.Password, false);

            if (result.Succeeded)
            {
                var appuser = _mapper.Map<UserForListDto>(user);

                return Ok(new
                {
                    token = GenerateJWTToken(user).Result,
                    user = appuser
                });
            }
            return Unauthorized();
        }

        private async Task<string> GenerateJWTToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.UserName)
            };

            var roles =await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role,role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }


    }
}