using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _repo = repo;
            _config = config;
            _mapper = mapper;
        }

        [HttpPost("Register")]

        public async Task<IActionResult> Register(UserForRegisterDto userForRegister)
        {
            //We Dont need to validate model state if use [APIController]
            //Validate Request
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);
            userForRegister.UserName = userForRegister.UserName.ToLower();

            if (await _repo.UserExist(userForRegister.UserName))
                return BadRequest(" Username already exists");

            var userToCreate = _mapper.Map<User>(userForRegister);


            var createdUser = await _repo.Register(userToCreate, userForRegister.Password);

            var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);

             return CreatedAtRoute("GetUser", new { controller ="Users" , id= createdUser.Id }, userToReturn);
           // return StatusCode(201); just to work code first time 

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
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = creds
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
            var user = _mapper.Map<UserForListDto>(userFromRepo);
                return Ok(new
                {

                    token = tokenHandler.WriteToken(token),
                    user
                });

            
           
        }
    }
}