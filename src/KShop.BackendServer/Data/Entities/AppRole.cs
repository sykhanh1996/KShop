using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace KShop.BackendServer.Data.Entities
{
    public class AppRole : IdentityRole
    {
        public AppRole()
        {
        }

        public AppRole(string id, string name)
        {
            Id = id;
            Name = name;
            NormalizedName = name.ToUpper();
        }

        public ICollection<Permission> Permissions { get; set; }
    }
}