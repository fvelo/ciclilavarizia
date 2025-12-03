using Ciclilavarizia.Models.Dtos;

namespace Ciclilavarizia.Services
{
    public class CAndPStore
    {
        public List<CustomerDetailDto> _customers { get; set; } = new List<CustomerDetailDto>();
        public List<ProductDto> _products { get; set; } = new List<ProductDto>();
    }

    public static class CAndPStoreServiceExtention{
        public static IServiceCollection AddCAndPStore(this IServiceCollection services)
        {
            services.AddSingleton<CAndPStore>();
            return services;
        }
    }
}

