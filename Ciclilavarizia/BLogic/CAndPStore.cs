using Ciclilavarizia.Models.Dtos;

namespace WebBetacomeDbFirst.BLogic
{
    public class CAndPStore
    {
        public List<CustomerDto> _customers { get; set; } = new List<CustomerDto>();
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

