namespace Ciclilavarizia.Services.ServicesExtentions
{
    public static class ProductsServiceExtention
    {
        public static IServiceCollection AddProductsService(this IServiceCollection services)
        {
            services.AddScoped<IProductsService, ProductsService>();
            return services;
        }
    }
}
