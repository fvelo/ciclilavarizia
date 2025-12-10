using ChatLib;
using Ciclilavarizia.Services.Interfaces;

namespace Ciclilavarizia.Services.ServicesExtentions
{
    public static class CustomersServiceExtention
    {
        public static IServiceCollection AddCustomersService(this IServiceCollection services)
        {
            services.AddScoped<ICustomersService, CustomersService>();
            return services;
        }
    }
}