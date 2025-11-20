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
