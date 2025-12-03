namespace Ciclilavarizia.Services.ServicesExtentions
{
    public static class LoginServiceExtention
    {
        public static IServiceCollection AddLoginService(this IServiceCollection services)
        {
            services.AddScoped<LoginService>();
            return services;
        }
    }
}
