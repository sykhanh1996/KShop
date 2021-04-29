using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KShop.BackendServer.Data.Entities;
using KShop.BackendServer.Data.Extension;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KShop.BackendServer.Data.Configuration
{
    public class PermissionConfiguration : DbEntityConfiguration<Permission>
    {
        public override void Configure(EntityTypeBuilder<Permission> entity)
        {
            entity.HasKey(c => new { c.RoleId, c.FunctionId, c.CommandId });
            // etc.
        }
    }
}
