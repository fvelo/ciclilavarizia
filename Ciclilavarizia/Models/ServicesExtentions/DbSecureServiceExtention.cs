using DataAccessLayer;

namespace Ciclilavarizia.Models.ServicesExtentions
{
    public static class DbSecureServiceExtention
    {
        public static IServiceCollection AddDbSecure(this IServiceCollection services, string ccnString)
        {
            DbSecure dbSecure = new(ccnString);
            services.AddSingleton(dbSecure);
            return services;
        }
    }
}
