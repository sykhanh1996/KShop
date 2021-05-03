using System;
using System.Collections.Generic;
using System.Text;

namespace KShop.ViewModels.Systems
{
    public class CommandAssignRequest
    {
        public string[] CommandIds { get; set; }

        public bool AddToAllFunctions { get; set; }
    }
}
