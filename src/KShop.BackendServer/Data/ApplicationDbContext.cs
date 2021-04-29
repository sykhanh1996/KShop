using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KShop.BackendServer.Data.Configuration;
using KShop.BackendServer.Data.Entities;
using KShop.BackendServer.Data.Extension;
using KShop.BackendServer.Data.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KShop.BackendServer.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public string UserId { get; set; }
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<ActivityLog> ActivityLogs { set; get; }
        public DbSet<Command> Commands { set; get; }
        public DbSet<CommandInFunction> CommandInFunctions { set; get; }
        public DbSet<Function> Functions { set; get; }
        public DbSet<Permission> Permissions { set; get; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Identity Configuration

            builder.Entity<AppRole>().Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
            builder.Entity<AppUser>().Property(x => x.Id).HasMaxLength(50).IsUnicode(false);

            #endregion Identity Configuration


            builder.AddConfiguration(new CommandInFunctionConfiguration());
            builder.AddConfiguration(new PermissionConfiguration());


            builder.HasSequence("ProductSequence");
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<EntityEntry> modified = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
            foreach (EntityEntry item in modified)
            {
                if (item.Entity is IDateTracking changedOrAddedItem)
                {
                    if (item.State == EntityState.Added)
                    {
                        changedOrAddedItem.CreateDate = DateTime.Now;
                        if (!string.IsNullOrEmpty(UserId)) changedOrAddedItem.CreatedBy = UserId;
                    }
                    else
                    {
                        changedOrAddedItem.LastModifiedDate = DateTime.Now;
                        if (!string.IsNullOrEmpty(UserId)) changedOrAddedItem.ModifiedBy = UserId;
                    }
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
