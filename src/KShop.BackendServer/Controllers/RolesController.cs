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

namespace KShop.BackendServer.Controllers
{
    public class RolesController : BaseController
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        public RolesController(RoleManager<AppRole> roleManager,
            IMapper mapper,
            ApplicationDbContext context
           )
        {
            _roleManager = roleManager;
            _mapper = mapper;
            _context = context;
        }

        [HttpPost]
        //[ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.CREATE)]
        //[ApiValidationFilter]
        public async Task<IActionResult> PostRole(RoleCreateRequest request)
        {
            var role = _mapper.Map<AppRole>(request);
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, request);
            }

            //return BadRequest(new ApiBadRequestResponse(result));
            return BadRequest();
        }

        [HttpGet]
        //[ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.VIEW)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = _roleManager.Roles;
            var rolevms = await _mapper.ProjectTo<RoleVm>(roles).ToListAsync();
            return Ok(rolevms);
        }

        [HttpGet("filter")]
        //[ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.VIEW)]
        public async Task<IActionResult> GetRolesPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _roleManager.Roles;
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Id.Contains(filter) || x.Name.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);

            var lstRoleVms = await _mapper.ProjectTo<RoleVm>(items).ToListAsync();

            var pagination = new Pagination<RoleVm>
            {
                Items = lstRoleVms,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        [HttpGet("{id}")]
        //[ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.VIEW)]
        public async Task<IActionResult> GetById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                //return NotFound(new ApiNotFoundResponse(_localizer["FindRoleErr"] + id));
                return NotFound();

            var roleVm = _mapper.Map<RoleVm>(role);
            return Ok(roleVm);
        }

        [HttpPut("{id}")]
        //[ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.UPDATE)]
        //[ApiValidationFilter]
        public async Task<IActionResult> PutRole(string id, [FromBody] RoleCreateRequest roleVm)
        {
            if (id != roleVm.Id)
                //return BadRequest(new ApiBadRequestResponse(_localizer["RoleNotMatch"]));
                return BadRequest();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                //return NotFound(new ApiNotFoundResponse(_localizer["FindRoleErr"] + id));
                return NotFound();

            role.Name = roleVm.Name;
            role.NormalizedName = roleVm.Name.ToUpper();

            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return NoContent();
            }
            //return BadRequest(new ApiBadRequestResponse(result));
            return BadRequest();
        }

        [HttpDelete("{id}")]
        //[ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                //return NotFound(new ApiNotFoundResponse(_localizer["FindRoleErr"] + id));
                return NotFound();

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                var rolevm = _mapper.Map<RoleVm>(role);
                return Ok(rolevm);
            }
            //return BadRequest(new ApiBadRequestResponse(result));
            return BadRequest();
        }

        [HttpGet("{roleId}/permissions")]
        //[ClaimRequirement(FunctionCode.SYSTEM_PERMISSION, CommandCode.VIEW)]
        public async Task<IActionResult> GetPermissionByRoleId(string roleId)
        {
            var permissions = from p in _context.Permissions

                join a in _context.Commands
                    on p.CommandId equals a.Id
                where p.RoleId == roleId
                select new PermissionVm()
                {
                    FunctionId = p.FunctionId,
                    CommandId = p.CommandId,
                    RoleId = p.RoleId
                };

            return Ok(await permissions.ToListAsync());
        }

        [HttpPut("{roleId}/permissions")]
        //[ClaimRequirement(FunctionCode.SYSTEM_PERMISSION, CommandCode.VIEW)]
        //[ApiValidationFilter]
        public async Task<IActionResult> PutPermissionByRoleId(string roleId, [FromBody] UpdatePermissionRequest request)
        {
            //create new permission list from user changed
            var newPermissions = new List<Permission>();
            foreach (var p in request.Permissions)
            {
                newPermissions.Add(new Permission(p.FunctionId, roleId, p.CommandId));
            }
            var existingPermissions = _context.Permissions.Where(x => x.RoleId == roleId);

            _context.Permissions.RemoveRange(existingPermissions);
            _context.Permissions.AddRange(newPermissions.Distinct(new MyPermissionComparer()));
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }

            //return BadRequest(new ApiBadRequestResponse(_localizer["SavePerrmissionErr"]));
            return BadRequest();
        }
    }
    internal class MyPermissionComparer : IEqualityComparer<Permission>
    {
        // Items are equal if their ids are equal.
        public bool Equals(Permission x, Permission y)
        {
            // Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            // Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the items properties are equal.
            return x.CommandId == y.CommandId && x.FunctionId == y.FunctionId && x.RoleId == y.RoleId;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(Permission permission)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(permission, null)) return 0;

            //Get hash code for the ID field.
            int hashProductId = (permission.CommandId + permission.FunctionId + permission.RoleId).GetHashCode();

            return hashProductId;
        }
    }
}
