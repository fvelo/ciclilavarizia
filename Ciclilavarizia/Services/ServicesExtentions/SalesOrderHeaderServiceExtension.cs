namespace Ciclilavarizia.Services.ServicesExtentions
{
    public static class SalesOrderHeaderServiceExtension
    {
        public static IServiceCollection AddSalesOrderHeaderService(this IServiceCollection services)
        {
            services.AddScoped<ISalesOrderHeaderService, SalesOrderHeaderService>();
            return services;
        }
    }
}
