using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KShop.BackendServer.Helpers;
using Microsoft.AspNetCore.Builder;

namespace KShop.BackendServer.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorWrapping(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorWrappingMiddleware>();
        }
    }
}
