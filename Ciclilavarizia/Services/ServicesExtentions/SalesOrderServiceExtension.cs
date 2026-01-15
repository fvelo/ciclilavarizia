using Ciclilavarizia.Models.Settings;

namespace Ciclilavarizia.Services.ServicesExtentions
{
    public static class SalesOrderServiceExtension
    {
        public static IServiceCollection AddSalesOrderExtension(this IServiceCollection services)
        {
            services.AddScoped<SalesOrderService>();
            return services;
        }
    }
}
