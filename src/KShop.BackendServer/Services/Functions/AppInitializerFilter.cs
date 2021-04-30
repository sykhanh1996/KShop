using KShop.BackendServer.Data;
using KShop.BackendServer.Data.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KShop.BackendServer.Services.Functions
{
    public class AppInitializerFilter : IAsyncActionFilter
    {
        private ApplicationDbContext _dbContext;

        public AppInitializerFilter(
            ApplicationDbContext dbContext
        )
        {
            _dbContext = dbContext;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next
        )
        {
            string userId = null;
            var claimsIdentity = (ClaimsIdentity)context.HttpContext.User.Identity;

            var userIdClaim = claimsIdentity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                userId = userIdClaim.Value;
            }
            _dbContext.UserId = userId;

            await next();
        }
    }

    public static class ChangeTrackerExtensions
    {
        public static void ProcessCreation(this ChangeTracker changeTracker, string userId)
        {
            foreach (var item in changeTracker.Entries<IDateTracking>().Where(e => e.State == EntityState.Added))
            {
                item.Entity.CreatedBy = userId;
            }
        }
    }
}