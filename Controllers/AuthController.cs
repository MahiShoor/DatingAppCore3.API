using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Models;
using DatingAppCore3.API.Data;
using DatingAppCore3.API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingAppCore3.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController] //this attribute enable validations using attributes on DTO Class
    public class AuthController:ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repo,IConfiguration config)
        {
            _repo = repo;
          _config = config;
        }

        [HttpPost("Register")]

        public async  Task<IActionResult> Register( UserForRegisterDto userForRegister)
        {
            //We Dont need to validate model state if use [APIController]
            //Validate Request
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);
            userForRegister.UserName = userForRegister.UserName.ToLower();

            if (await _repo.UserExist(userForRegister.UserName))
                return BadRequest(" Username already exists");

            var userToCreate = new User
            {

                UserName = userForRegister.UserName
            };

            var createdUser = await _repo.Register(userToCreate,userForRegister.Password);

            // return CreatedAtRouteResult();
            return StatusCode(201);

        }
    
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _repo.Login(userForLoginDto.UserName, userForLoginDto.Password);
            if (userFromRepo == null)
            {
                return Unauthorized();

            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {

                token = tokenHandler.WriteToken(token)
            }); 
        }
    }
}