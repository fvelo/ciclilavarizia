using Ciclilavarizia.Models.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Ciclilavarizia.BLogic
{
    public static class AuthenticationServicesOptionsConfiguration
    {
        public static IServiceCollection AddAuthenticationOptions(this IServiceCollection services)
        {
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptionsMonitor<JwtSettings>>((jwtBearerOptions, jwtSettingsMonitor) =>
            {
                var jwtSettings = jwtSettingsMonitor.CurrentValue;
                var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromSeconds(3)
                };
            });

            // Now add authentication/authorization (this will use the JwtBearerOptions configured above)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(); // no lambda needed because options are configured above

            return services;
        }
    }
}
