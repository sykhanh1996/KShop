using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KShop.BackendServer.Constants;
using Microsoft.AspNetCore.Mvc;

namespace KShop.BackendServer.Authorization
{
    public class ClaimRequirementAttribute : TypeFilterAttribute
    {
        public ClaimRequirementAttribute(FunctionCode functionId, CommandCode commandId)
            : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] { functionId, commandId };
        }
    }
}
