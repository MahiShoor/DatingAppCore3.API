using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Models;
using DatingAppCore3.API.Dtos;
using DatingAppCore3.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DatingAppCore3.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _dataCotext;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private readonly Cloudinary _cloudinary;
        public AdminController( DataContext dataCotext, UserManager<User> userManager,
                  IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _dataCotext = dataCotext;
            _userManager = userManager;
            _cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret);

            _cloudinary = new Cloudinary(acc);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("userWithRoles")]
        public async Task<IActionResult> GetUserWithRole()
        {
            var userList = await _dataCotext.Users.OrderBy(x => x.UserName)
                .Select(user => new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Roles = (from userRole in user.UserRoles
                             join role in _dataCotext.Roles
                             on userRole.RoleId
                              equals role.Id
                             select role.Name).ToList()
                }).ToListAsync();
            return Ok(userList);
        }


        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("editRoles/{userName}")]
        
        public async Task<IActionResult> EditRole(string userName, RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);

            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = roleEditDto.RoleNames;

            // ?? clled as null coalescing short hand of below
            // var selectedRoles = selectedRoles !=null ? selectedRoles : new string [] {};
            selectedRoles = selectedRoles ?? new string[] { };

            var resulte = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!resulte.Succeeded)
           
                return BadRequest("Unable to add roles");
            resulte = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!resulte.Succeeded)
                BadRequest("Unable to remove roles");

            return Ok(await _userManager.GetRolesAsync(user));
            
        }
        
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public async  Task<IActionResult> GetPhotoForModeration()
        {
            var photos = await _dataCotext.Photos
                .Include(u => u.User)
                .IgnoreQueryFilters()
                .Where(p => p.IsApproved == false)
                .Select(u => new
                {
                    Id = u.Id,
                    UserName = u.User.UserName,
                    Url = u.Url,
                    IsApproved = u.IsApproved
                }).ToListAsync();
            return Ok(photos);
        }
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approvedPhoto/{photoId}")]

        public async Task<IActionResult> ApprovedPhoto(int photoId)
        {
            var photo = await _dataCotext.Photos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == photoId);
            photo.IsApproved = true;

            await _dataCotext.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("rejectPhoto/{photoId}")]

        public async Task<IActionResult> RejectPhoto(int photoId)
        {
            var photo = await _dataCotext.Photos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == photoId);
            photo.IsApproved = true;

            if (photo.IsMain)
            {
                return BadRequest("You can not reject main photo");
            }
            if(photo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photo.PublicId);

                var result = _cloudinary.Destroy(deleteParams);

                if(result.Result == "ok")
                {
                    _dataCotext.Remove(photo);
                }
            }

            if(photo.PublicId == null)
            {
                _dataCotext.Remove(photo);
            }

            await _dataCotext.SaveChangesAsync();

            return Ok();
        }
    }


}       