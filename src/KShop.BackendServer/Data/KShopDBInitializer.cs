using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KShop.BackendServer.Data.Entities;
using KShop.BackendServer.Data.Enums;
using Microsoft.AspNetCore.Identity;

namespace KShop.BackendServer.Data
{
    public class KShopDBInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly string AdminRoleName = "Admin";
        private readonly string UserRoleName = "Member";

        public KShopDBInitializer(ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Seed()
        {
            #region Quyền

            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new AppRole
                {
                    Id = AdminRoleName,
                    Name = AdminRoleName,
                    NormalizedName = AdminRoleName.ToUpper(),
                });
                await _roleManager.CreateAsync(new AppRole
                {
                    Id = UserRoleName,
                    Name = UserRoleName,
                    NormalizedName = UserRoleName.ToUpper(),
                });
            }

            #endregion Quyền

            #region Người dùng

            if (!_userManager.Users.Any())
            {
                var result = await _userManager.CreateAsync(new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "admin",
                    FirstName = "Quản trị",
                    LastName = "Web",
                    FullName = "Nguyễn Sỹ Khánh",
                    Email = "sykhanh1996@gmail.com",
                    LockoutEnabled = false
                }, "Admin@123");
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync("admin");
                    await _userManager.AddToRoleAsync(user, AdminRoleName);
                }
            }

            #endregion Người dùng


            #region Chức năng

            if (!_context.Functions.Any())
            {
                _context.Functions.AddRange(new List<Function>
                {
                    new Function {Id = "DASHBOARD",  ParentId = null,Name = "Thống kê",SortOrder = 1,Url = "/dashboard" },

                    new Function {Id = "PRODUCT",ParentId = null,Name = "Sản phẩm",Url = "/products" },

                    new Function {Id = "CATEGORY",ParentId = null,Name = "Thể loại",Url = "/categories" },

                    new Function {Id = "BILL",ParentId = null,Name = "Hóa đơn",Url = "/bills"  },

                    new Function {Id = "SYSTEM", ParentId = null,Name = "Hệ thống", Url = "/systems"},

                    new Function {Id = "SYSTEM_USER", ParentId = "SYSTEM",Name = "Người dùng", Url = "/systems/users"},
                    new Function {Id = "SYSTEM_ROLE", ParentId = "SYSTEM",Name = "Nhóm Quyền", Url = "/systems/roles"},
                    new Function {Id = "SYSTEM_FUNCTION",ParentId = "SYSTEM",Name = "Chức năng", Url = "/systems/functions"},
                    new Function {Id = "SYSTEM_PERMISSION",ParentId = "SYSTEM",Name = "Quyền hạn", Url = "/systems/permissions"}
                });
                await _context.SaveChangesAsync();
            }

            if (!_context.Commands.Any())
            {
                _context.Commands.AddRange(new List<Command>
                {
                    new Command(){Id = "VIEW", Name = "Xem"},
                    new Command(){Id = "CREATE", Name = "Thêm"},
                    new Command(){Id = "UPDATE", Name = "Sửa"},
                    new Command(){Id = "DELETE", Name = "Xoá"},
                    new Command(){Id = "APPROVE", Name = "Duyệt"},
                });
            }

            #endregion Chức năng


            var functions = _context.Functions;

            if (!_context.CommandInFunctions.Any())
            {
                foreach (var function in functions)
                {
                    var createAction = new CommandInFunction()
                    {
                        CommandId = "CREATE",
                        FunctionId = function.Id
                    };
                    _context.CommandInFunctions.Add(createAction);

                    var updateAction = new CommandInFunction()
                    {
                        CommandId = "UPDATE",
                        FunctionId = function.Id
                    };
                    _context.CommandInFunctions.Add(updateAction);
                    var deleteAction = new CommandInFunction()
                    {
                        CommandId = "DELETE",
                        FunctionId = function.Id
                    };
                    _context.CommandInFunctions.Add(deleteAction);

                    var viewAction = new CommandInFunction()
                    {
                        CommandId = "VIEW",
                        FunctionId = function.Id
                    };
                    _context.CommandInFunctions.Add(viewAction);
                }
            }

            if (!_context.Permissions.Any())
            {
                var adminRole = await _roleManager.FindByNameAsync(AdminRoleName);
                foreach (var function in functions)
                {
                    _context.Permissions.Add(new Permission(function.Id, adminRole.Id, "CREATE"));
                    _context.Permissions.Add(new Permission(function.Id, adminRole.Id, "UPDATE"));
                    _context.Permissions.Add(new Permission(function.Id, adminRole.Id, "DELETE"));
                    _context.Permissions.Add(new Permission(function.Id, adminRole.Id, "VIEW"));
                    _context.Permissions.Add(new Permission(function.Id, adminRole.Id, "APPROVE"));
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
