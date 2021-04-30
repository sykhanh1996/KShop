using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KShop.BackendServer.Data.Entities;
using KShop.ViewModels.Systems;

namespace KShop.BackendServer.AutoMapper
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile()
        {
            CreateMap<AppRole, RoleVm>().MaxDepth(2);
        }
    }
}
