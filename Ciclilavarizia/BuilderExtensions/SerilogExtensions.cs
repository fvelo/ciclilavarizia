using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace Ciclilavarizia.BuilderExtensions
{
    public static class SerilogExtensions
    {
        public static void AddSerilogConfiguration(this WebApplicationBuilder builder, string connectionString)
        {
            // Define the base path for logs
            string logPath = Path.Combine(AppContext.BaseDirectory, "Logs", DateTime.Now.ToString("yyyy-MM"));

            var sinkOptions = new MSSqlServerSinkOptions
            {
                TableName = "ExeptionLogs",
                AutoCreateSqlTable = true
            };

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning) // Reduce noise
                .Enrich.FromLogContext()

                // Console Sink
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)

                // SQL SERVER SINK (All Warnings and Errors)
                .WriteTo.MSSqlServer(
                    connectionString: builder.Configuration.GetConnectionString(connectionString),
                    sinkOptions: sinkOptions,
                    restrictedToMinimumLevel: LogEventLevel.Warning)

                // DEBUG FILE (Only Debug)
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
                    .WriteTo.File(Path.Combine(logPath, "debug-.txt"), rollingInterval: RollingInterval.Month))

                // INFO FILE (Info + Warning)
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information || e.Level == LogEventLevel.Warning)
                    .WriteTo.File(Path.Combine(logPath, "info-.txt"), rollingInterval: RollingInterval.Month))

                // ERROR FILE (Error + Fatal)
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                    .WriteTo.File(Path.Combine(logPath, "exceptions-.txt"), rollingInterval: RollingInterval.Month))

                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
