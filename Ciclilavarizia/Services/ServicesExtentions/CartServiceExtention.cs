namespace Ciclilavarizia.Services.ServicesExtentions
{
    public static class CartServiceExtention
    {
        public static IServiceCollection AddCartService(this IServiceCollection services)
        {
            services.AddScoped<CartService>();
            return services;
        }
    }
}