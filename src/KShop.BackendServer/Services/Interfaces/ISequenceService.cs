using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KShop.BackendServer.Services.Interfaces
{
    public interface ISequenceService
    {
        Task<int> GetProductNewId();
    }
}
