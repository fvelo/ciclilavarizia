namespace Ciclilavarizia.Services.ServicesExtentions
{
    public static class MDBServiceExtention
    {
        public static IServiceCollection AddMDBService(this IServiceCollection services)
        {
            services.AddScoped<CartService>();
            return services;
        }
    }
}