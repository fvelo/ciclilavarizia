using Ciclilavarizia.Data;
using Ciclilavarizia.Models.Settings;
using Ciclilavarizia.Services;
using Ciclilavarizia.Services.ServicesExtentions;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;


namespace Ciclilavarizia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(opt =>
            {
                opt.RespectBrowserAcceptHeader = true;
                //opt.OutputFormatters.RemoveType<StringOutputFormatter>();
            })
                .AddXmlSerializerFormatters();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // bind IOptions
            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection(nameof(JwtSettings)));

            //db connection

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContext<AdventureWorksLTContext>(o =>
                    o.UseSqlServer(builder.Configuration.GetConnectionString("CiclilavariziaDev")));
                builder.Services.AddDbSecure(
                    builder.Configuration.GetConnectionString("CiclilavariziaSecureDev"));
            }else if (builder.Environment.IsProduction())
            {
                //builder.Services.AddDbContext<AdventureWorksLTContext>(o =>
                //    o.UseSqlServer(builder.Configuration.GetConnectionString("CiclilavariziaDev")));
                //builder.Services.AddDbSecure(
                //    builder.Configuration.GetConnectionString("CiclilavariziaSecureDev"));
            }


            //builder.Services.AddDbSecure(
            //builder.Configuration.GetConnectionString("AdventureWorksSecureDbHomelab") ?? string.Empty);

            // CORS
            var AnyOrigin = "_anyOrigin";
            var LiveServerOrigin = "_liveServerOrigin";
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy(name: LiveServerOrigin, policy =>
                    {
                        policy.AllowAnyOrigin()
                        //policy.WithOrigins("http://localhost:4200") // this is the SPA made with angular
                        //.AllowAnyHeader()
                        .AllowAnyMethod();
                    });
                });
            }

            //
            // Start Custom Services Extentions
            //

            // Service for exercises

            builder.Services.AddAuthenticationOptions();
            builder.Services.AddAuthorizationOptions();

            builder.Services.AddCAndPStore();
            builder.Services.AddCustomerService();

            //
            // End Custom Services Extentions
            //

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            if (builder.Environment.IsDevelopment())
            {
                app.UseCors(LiveServerOrigin);
            }

            app.MapControllers();
            app.Run();
        }
    }
}
