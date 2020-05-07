using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Datingapp.API.Data;
using Datingapp.API.Dtos;
using Datingapp.API.Helpers;
using Datingapp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Datingapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context1;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public AdminController(DataContext context, UserManager<User> userManager, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _context = context;
            _userManager = userManager;
            _cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account()
            {
                ApiKey = _cloudinaryConfig.Value.ApiKey,
                ApiSecret = _cloudinaryConfig.Value.ApiSecret,
                Cloud = _cloudinaryConfig.Value.CloudName
            };

            _cloudinary = new Cloudinary(acc);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("EditRole/{UserName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var roles = await _userManager.GetRolesAsync(user);

            var selecetRoles = roleEditDto.RoleNames;

            selecetRoles = selecetRoles ?? new string[] { };

            var result = await _userManager.AddToRolesAsync(user, selecetRoles.Except(roles));
            if (!result.Succeeded)
                return BadRequest("Failed to add the roles");

            result = await _userManager.RemoveFromRolesAsync(user, roles.Except(selecetRoles));

            if (!result.Succeeded)
                return BadRequest("Failed to remove the roles");

            return Ok(await _userManager.GetRolesAsync(user));

        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("UserWithRoles")]
        public async Task<IActionResult> GetUsersWithRols()
        {
            var users = await _context.Users
                .OrderBy(d => d.UserName)
                .Select(d => new
                {
                    Id = d.Id,
                    userName = d.UserName,
                    Roles = (from userRole in d.UserRoles
                             join role in _context.Roles
                             on userRole.RoleId equals role.Id
                             select role.Name).ToList()
                }).ToListAsync();
            return Ok(users);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("PhotosForModeration")]
        public async Task<IActionResult> GetPhotosForModeration()
        {
            var photos = await _context.Photos
                        .Include(d => d.User)
                        .IgnoreQueryFilters()
                        .Where(p => p.IsAproved == false)
                        .Select(u => new
                        {
                            Id = u.Id,
                            UserName = u.User.UserName,
                            Url = u.Url,
                            IsAproved = u.IsAproved

                        }).ToListAsync();

            return Ok(photos);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approvePhoto/{photoId}")]
        public async Task<IActionResult> ApprovedPhoto(int photoId)
        {
            var photo = await _context.Photos.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Id == photoId);
            photo.IsAproved = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("rejectPhoto/{photoId}")]
        public async Task<IActionResult> RejectPhoto(int photoId)
        {
            var photo = await _context.Photos.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Id == photoId);

            if (photo.IsMain)
                return BadRequest("you can not reject the main photo");

            if (photo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);
                if (result.Result == "ok")
                {
                    _context.Photos.Remove(photo);
                }
            }
            if (photo.PublicId == null)
            {
                _context.Photos.Remove(photo);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}