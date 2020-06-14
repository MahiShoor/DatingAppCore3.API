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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingAppCore3.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController] //this attribute enable validations using attributes on DTO Class
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
       // private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(//IAuthRepository repo,
            IConfiguration config, IMapper mapper,UserManager<User> userManager
            ,SignInManager<User> signInManager)
        {
           // _repo = repo;
            _config = config;
            _mapper = mapper;
           _userManager = userManager;
           _signInManager = signInManager;
        }

        [HttpPost("Register")]

        public async Task<IActionResult> Register(UserForRegisterDto userForRegister)
        {
            //We Dont need to validate model state if use [APIController]
            //Validate Request
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);
            //userForRegister.UserName = userForRegister.UserName.ToLower();

            //if (await _repo.UserExist(userForRegister.UserName))
            //    return BadRequest(" Username already exists");



            var userToCreate = _mapper.Map<User>(userForRegister);

            var result = await _userManager.CreateAsync(userToCreate,userForRegister.Password);

          //  var createdUser = await _repo.Register(userToCreate, userForRegister.Password);

            var userToReturn = _mapper.Map<UserForDetailedDto>(userToCreate);
            if (result.Succeeded)
            {
                return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userToReturn);
            }

            return BadRequest(result.Errors);
           // return StatusCode(201); just to work code first time 

        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            //var userFromRepo = await _repo.Login(userForLoginDto.UserName, userForLoginDto.Password);
            //     if (userFromRepo == null)
            //     {
            //         return Unauthorized();

            //     }
            // var user = _mapper.Map<UserForListDto>(userFromRepo);
            //     return Ok(new
            //     {

            //         token = GenerateJwtToken(userFromRepo),
            //         user
            //     });
            var user = await _userManager.FindByNameAsync(userForLoginDto.UserName);
            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);
            if (result.Succeeded)
            {
                var appUser = _mapper.Map<UserForListDto>(user);
                return Ok(new
                {

                    token = GenerateJwtToken(user).Result,
                    user = appUser
                });
            }
         
          
                return Unauthorized();

        }

        private async Task<string>  GenerateJwtToken(User userFromRepo)
        {
            var claims = new List<Claim>
             {
                new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.UserName)
            };

            var roles = await _userManager.GetRolesAsync(userFromRepo);

            foreach(var role in roles)
            {
                claims.Add(new Claim (ClaimTypes.Role,role));
            }

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
            return tokenHandler.WriteToken(token);
        }
    }
}