using Ciclilavarizia.Filters;
using Ciclilavarizia.Models.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Ciclilavarizia.Services.ServicesExtentions
{
    public static class ActionFiltersExtention
    {
        public static IServiceCollection AddCustomActionFilters(this IServiceCollection services)
        {
            services.AddTransient<EnsureCustomerExistsFilter>();
            services.AddTransient<EnsureProductExistsFilter>();
            return services;
        }
    }
}
