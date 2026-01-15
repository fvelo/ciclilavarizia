namespace Ciclilavarizia.Services.ServicesExtentions
{
    public static class SalesOrderServiceExtention
    {
        public static IServiceCollection AddLoginService(this IServiceCollection services)
        {
            services.AddScoped<LoginService>();
            return services;
        }
    }
}
