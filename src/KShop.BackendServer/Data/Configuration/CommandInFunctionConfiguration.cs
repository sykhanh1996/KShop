using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KShop.BackendServer.Data.Entities;
using KShop.BackendServer.Data.Extension;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KShop.BackendServer.Data.Configuration
{
    public class CommandInFunctionConfiguration : DbEntityConfiguration<CommandInFunction>
    {
        public override void Configure(EntityTypeBuilder<CommandInFunction> entity)
        {
            entity.HasKey(c => new { c.CommandId, c.FunctionId });
            // etc.
        }
    }
}
