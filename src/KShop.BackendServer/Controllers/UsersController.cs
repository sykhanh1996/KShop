using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KShop.BackendServer.Data;
using KShop.BackendServer.Data.Entities;
using KShop.ViewModels;
using KShop.ViewModels.Systems;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KShop.BackendServer.Constants;

namespace KShop.BackendServer.Controllers
{
    public class UsersController : BaseController
    {
        private readonly UserManager<AppUser> _userManager;

        private readonly RoleManager<AppRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UsersController(UserManager<AppUser> userManager,

            RoleManager<AppRole> roleManager,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _mapper = mapper;

        }
        [HttpPost]
        //[ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.CREATE)]
        //[ApiValidationFilter]
        public async Task<IActionResult> PostUser(UserCreateRequest request)
        {
            //var culture = CultureInfo.CurrentCulture.Name;
            var user = _mapper.Map<AppUser>(request);

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, request);

            //return BadRequest(new ApiBadRequestResponse(result));
            return BadRequest();
        }

        [HttpGet]
        //[ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users;
            var uservms = await _mapper.ProjectTo<UserVm>(users).ToListAsync();

            return Ok(uservms);
        }

        [HttpGet("filter")]
        //[ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> GetUsersPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _userManager.Users;
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Email.Contains(filter)
                || x.UserName.Contains(filter)
                || x.PhoneNumber.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);

            var lstUserVm = await _mapper.ProjectTo<UserVm>(items).ToListAsync();

            var pagination = new Pagination<UserVm>
            {
                Items = lstUserVm,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        //URL: GET: http://localhost:5001/api/users/{id}
        [HttpGet("{id}")]
        //[ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userVm = _mapper.Map<UserVm>(user);
            return Ok(userVm);
        }

        [HttpPut("{id}")]
        //[ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
        public async Task<IActionResult> PutUser(string id, [FromBody] UserCreateRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                //return NotFound(new ApiNotFoundResponse(_localizer["FindUserIdError"] + " " + id));
                return NotFound("Cannot found user with Id: " + id);

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Dob = request.Dob;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }
            //return BadRequest(new ApiBadRequestResponse(result));
            return BadRequest();
        }

        [HttpPut("{id}/change-password")]
        //[ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
        //[ApiValidationFilter]
        public async Task<IActionResult> PutUserPassword(string id, [FromBody] UserPasswordChangeRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
               //return NotFound(new ApiNotFoundResponse(_localizer["FindUserIdError"] + " " + id));
                return NotFound("Cannot found user with Id: "+id);

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                return NoContent();
            }
            //return BadRequest(new ApiBadRequestResponse(result));
            return BadRequest();
        }

        [HttpDelete("{id}")]
        //[ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var adminUsers = await _userManager.GetUsersInRoleAsync(SystemConstants.Roles.Admin);
            var otherUsers = adminUsers.Where(x => x.Id != id).ToList();
            if (otherUsers.Count == 0)
            {
                //return BadRequest(new ApiBadRequestResponse(_localizer["RemoveLastAdminErr"]));
                return BadRequest("You cannot remove the last admin user remaining");
            }
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                var uservm = _mapper.Map<UserVm>(user);
                return Ok(uservm);
            }
            //return BadRequest(new ApiBadRequestResponse(result));
            return BadRequest();
        }

        [HttpGet("{userId}/menu")]
        public async Task<IActionResult> GetMenuByUserPermission(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);
            var query = from f in _context.Functions
                        join p in _context.Permissions
                            on f.Id equals p.FunctionId
                        join r in _roleManager.Roles on p.RoleId equals r.Id
                        join a in _context.Commands
                            on p.CommandId equals a.Id
                        where roles.Contains(r.Name) && a.Id == "VIEW"
                        select new FunctionVm
                        {
                            Id = f.Id,
                            Name = f.Name,
                            Url = f.Url,
                            ParentId = f.ParentId,
                            SortOrder = f.SortOrder,
                            Icon = f.Icon
                        };
            var data = await query.Distinct()
                .OrderBy(x => x.ParentId)
                .ThenBy(x => x.SortOrder)
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("{userId}/roles")]
        //[ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                //return NotFound(new ApiNotFoundResponse(_localizer["FindUserIdError"] + " " + userId));
                return NotFound("Cannot found user with Id: " +userId);
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        [HttpPost("{userId}/roles")]
        //[ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
        public async Task<IActionResult> PostRolesToUserUser(string userId, [FromBody] RoleAssignRequest request)
        {
            if (request.RoleNames?.Length == 0)
            {
                //return BadRequest(new ApiBadRequestResponse(_localizer["RoleNameEmptyErr"]));
                return BadRequest("Role names cannot empty");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                //return NotFound(new ApiNotFoundResponse(_localizer["FindUserIdError"] + " " + userId));
                return NotFound("Cannot found user with Id: "+ userId);
            var result = await _userManager.AddToRolesAsync(user, request.RoleNames);
            if (result.Succeeded)
                return Ok();

            //return BadRequest(new ApiBadRequestResponse(result));
            return BadRequest();
        }

        [HttpDelete("{userId}/roles")]
        //[ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> RemoveRolesFromUser(string userId, [FromQuery] RoleAssignRequest request)
        {
            if (request.RoleNames?.Length == 0)
            {
                //return BadRequest(new ApiBadRequestResponse(_localizer["RoleNameEmptyErr"]));
                return BadRequest("Role names cannot empty");
            }
            if (request.RoleNames.Length == 1 && request.RoleNames[0] == Constants.SystemConstants.Roles.Admin)
            {
                //return base.BadRequest(new ApiBadRequestResponse(string.Format(_localizer["RemoveRoleErr"], Constants.SystemConstants.Roles.Admin)));
                return base.BadRequest(string.Format("Cannot remove {0} role", SystemConstants.Roles.Admin));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                //return NotFound(new ApiNotFoundResponse(_localizer["FindUserIdError"] + " " + userId));
                return NotFound("Cannot found user with Id:" + " " + userId);
            var result = await _userManager.RemoveFromRolesAsync(user, request.RoleNames);
            if (result.Succeeded)
                return Ok();

            //return BadRequest(new ApiBadRequestResponse(result));
            return BadRequest();
        }
    }
}
