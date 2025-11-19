namespace Ciclilavarizia.Services.ServicesExtentions
{
    public static class AuthorizationServicesOptionsConfiguration
    {
        public static IServiceCollection AddAuthorizationOptions(this IServiceCollection services)
        {
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("AdminPolicy", p => p.RequireRole("admin"));
                opt.AddPolicy("UserPolicy", p => p.RequireRole("user", "admin"));
            });
            return services;
        }
    }
}
