using Ciclilavarizia.Data;
using Ciclilavarizia.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ciclilavarizia.Filters
{
    //public class EnsureProductExists : Attribute, IAsyncActionFilter
    //{
    //    // TODO: use proper Dependency Injection and not this anti-pattern, how to do it on youtube brotherrrrr susu froza

    //    private readonly string _idParameterName;

    //    public EnsureProductExists(string idParameterName = "id")
    //    {
    //        _idParameterName = idParameterName;
    //    }

    //    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    //    {
    //        if (!context.ActionArguments.TryGetValue(_idParameterName, out object idValue) || idValue == null)
    //        {
    //            context.Result = new BadRequestObjectResult($"The Id must be provided.");
    //            //await next();
    //            return;
    //        }

    //        if (idValue is not int id || id < 0)
    //        {
    //            context.Result = new BadRequestObjectResult($"Invalid ID provided for {_idParameterName}.");
    //            return;
    //        }

    //        var dbContext = context.HttpContext.RequestServices.GetRequiredService<CiclilavariziaDevContext>();

    //        bool exists = await dbContext.Products
    //            .AsNoTracking()
    //            .AnyAsync(p => p.ProductID == id, context.HttpContext.RequestAborted);

    //        if (!exists)
    //        {
    //            context.Result = new NotFoundResult();
    //            return;
    //        }

    //        await next();
    //    }
    //}

    public class EnsureProductExistsFilter : IAsyncActionFilter
    {
        private readonly string _idParameterName;
        private readonly IProductsService _service;
        private readonly ILogger<EnsureProductExistsFilter> _logger;


        public EnsureProductExistsFilter(IProductsService service, ILogger<EnsureProductExistsFilter> logger, string idParameterName = "id")
        {
            _service = service;
            _logger = logger;
            _idParameterName = idParameterName;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.ContainsKey(_idParameterName))
            {
                _logger.LogError($"The _idParameterName contain the value: \"{_idParameterName}\", such value does not exixts in context.ActionArguments.");
                throw new ArgumentException(_idParameterName, nameof(context));
            }

            if (!context.ActionArguments.TryGetValue(_idParameterName, out object idValue) || idValue == null)
            {
                context.Result = new BadRequestObjectResult($"The Id must be provided.");
                return;
            }

            if (idValue is not int id || id < 0)
            {
                context.Result = new BadRequestObjectResult($"Invalid ID provided for {_idParameterName}.");
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<CiclilavariziaDevContext>();

            bool exists = await _service
                .DoesProductExistsAsync((int)idValue, context.HttpContext.RequestAborted);

            if (!exists)
            {
                context.Result = new NotFoundResult();
                return;
            }

            await next();
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class EnsureProductExistsAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        public string IdParameterName { get; init; } = "id";
        public int Order { get; set; } = 0;
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<EnsureProductExistsFilter>>();
            var service = serviceProvider.GetRequiredService<IProductsService>();

            return new EnsureProductExistsFilter(service, logger, IdParameterName);
        }
    }
}
