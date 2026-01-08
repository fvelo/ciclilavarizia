using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Ciclilavarizia.Services
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // 1. Check for Cancellation (Do not log as error, just return 499)
            if (exception is OperationCanceledException)
            {
                httpContext.Response.StatusCode = 499;
                return true;
            }

            // 2. Log only the 500-level (Unexpected) Exceptions
            // Because we used builder.Host.UseSerilog(), this call 
            // automatically goes to SQL Server and the 'operations' file.
            _logger.LogError(exception, "An unhandled error occurred in Ciclilavarizia: {Message}", exception.Message);

            // 3. Construct the 500 Response
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Internal Server Error",
                Detail = "A technical error occurred. The support team has been notified.",
                Instance = httpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] = httpContext.TraceIdentifier // Useful for finding the error in the SQL DB later
                }
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            // Return true to signal that this specific exception has been handled
            return true;

        }
    }
}
