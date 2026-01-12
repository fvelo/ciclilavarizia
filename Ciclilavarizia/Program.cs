using Ciclilavarizia.BuilderExtensions;
using Ciclilavarizia.Data;
using Ciclilavarizia.Models.Settings;
using Ciclilavarizia.Services;
using Ciclilavarizia.Services.ServicesExtentions;
using CommonCiclilavarizia;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

namespace Ciclilavarizia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddSerilogConfiguration("CiclilavariziaSecureDev");

            // 
            // Add services to the container.
            // 
            
            builder.Services.AddControllers(/*opt =>
            {
                opt.RespectBrowserAcceptHeader = true;
                //opt.OutputFormatters.RemoveType<StringOutputFormatter>();
            }*/)
                //.AddXmlSerializerFormatters()
            ; // this is removed for now, but it is needed to format request in other formats outside JSON, TODO: intorduce input/output formatters

            builder.Services.Configure<RouteOptions>((o) =>
            {
                o.LowercaseUrls = true;
                o.LowercaseQueryStrings = false;
                o.AppendTrailingSlash = true;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // bind IOptions
            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection(nameof(JwtSettings)));

            //db connection

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContext<CiclilavariziaDevContext>(o =>
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

            builder.Services.AddCustomersService();
            builder.Services.AddProductsService();
            builder.Services.AddLoginService();
            builder.Services.AddCustomActionFilters(); //needs to be after AddCustomersService because it depends on it
            builder.Services.AddCustomEncryptionService();
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


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
            // MUST be after security middleware and before MapControllers
            app.UseExceptionHandler();

            app.MapControllers();
            app.Run();
        }
    }
}
