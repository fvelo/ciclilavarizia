using Ciclilavarizia.Models.Settings;

namespace Ciclilavarizia.Services.ServicesExtentions
{
    public static class SalesOrderDetailsServiceExtension
    {
        public static IServiceCollection AddSalesOrderDetailsService(this IServiceCollection services)
        {
            services.AddScoped<SalesOrderDetailsService>();
            return services;
        }
    }
}
