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
            // Define the base path for logs
            string logPath = Path.Combine(AppContext.BaseDirectory, "Logs", DateTime.Now.ToString("yyyy-MM"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                // This allows the "Microsoft.AspNetCore" logs to pass through to the console
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()

                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                     theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)

                // SQL SERVER SINK (All Warnings and Errors)
                .WriteTo.MSSqlServer(
                    connectionString: builder.Configuration.GetConnectionString("CiclilavariziaDev"),
                    sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true },
                    restrictedToMinimumLevel: LogEventLevel.Warning)

                // DEBUG FILE (Only Debug)
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
                    .WriteTo.File(Path.Combine(logPath, "debug-.txt"), rollingInterval: RollingInterval.Month))
                
                // INFO FILE (Only Information)
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information || e.Level == LogEventLevel.Warning)
                    .WriteTo.File(Path.Combine(logPath, "info-.txt"), rollingInterval: RollingInterval.Month))

                // ERROR/REMAINING FILE (Warning, Error, Fatal)
                .WriteTo.Logger(lc => lc
                    .Filter.ByExcluding(e => e.Level == LogEventLevel.Information || e.Level == LogEventLevel.Debug)
                    .WriteTo.File(Path.Combine(logPath, "exeptions-.txt"), rollingInterval: RollingInterval.Month))
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add services to the container.

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

            builder.Services.AddCAndPStore();
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
