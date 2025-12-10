using Ciclilavarizia.Services;
using Ciclilavarizia.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ciclilavarizia.Filters
{
    public class EnsureCustomerExistsFilter : IAsyncActionFilter
    {
        private readonly ICustomersService _service;
        private readonly string _idParameterName;
        private readonly ILogger<EnsureCustomerExistsFilter> _logger;

        public EnsureCustomerExistsFilter(ICustomersService service, ILogger<EnsureCustomerExistsFilter> logger, string idParameterName = "id")
        {
            _service = service;
            _idParameterName = idParameterName;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.ContainsKey(_idParameterName))
            {
                _logger.LogError($"The _idParameterName contain the value: \"{_idParameterName}\", such value does not exixts in context.ActionArguments.");
                foreach (var arg in context.ActionArguments)
                {
                    Console.WriteLine($"ket: {arg.Key}, value: {arg.Value}");
                }

                throw new ArgumentException(_idParameterName, nameof(context));
            }

            int? id = (int?)context.ActionArguments[_idParameterName] ?? null;
            if (id == null || id < 0)
            {
                context.Result = new BadRequestObjectResult($"Invalid ID provided for customerId.");
                return;
            }

            if (!await _service.DoesCustomerExistsAsync((int)id, context.HttpContext.RequestAborted))
            {
                context.Result = new NotFoundResult();
                Console.WriteLine("Not found returned from EnsureCustomerExistsFilter!!!");
                return;
            }

            await next();
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class EnsureCustomerExistsAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        public string IdParameterName { get; init; } = "id";
        public int Order { get; set; } = 0;
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) // not anti-pattern because wrapped in an IFilterfactory
        {
            var service = serviceProvider.GetRequiredService<ICustomersService>();
            var logger = serviceProvider.GetRequiredService<ILogger<EnsureCustomerExistsFilter>>();
            return new EnsureCustomerExistsFilter(service, logger, IdParameterName);
        }
    }
}
