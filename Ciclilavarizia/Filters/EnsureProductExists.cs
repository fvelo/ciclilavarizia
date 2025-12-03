using Ciclilavarizia.Data;
using Ciclilavarizia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Filters
{
    public class EnsureProductExists : Attribute, IAsyncActionFilter
    {
        // TODO: use proper Dependency Injection and not this anti-pattern, how to do it on youtube brotherrrrr susu froza

        private readonly string _idParameterName;

        public EnsureProductExists(string idParameterName = "id")
        {
            _idParameterName = idParameterName;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue(_idParameterName, out object idValue) || idValue == null)
            {
                context.Result = new BadRequestObjectResult($"The Id must be provided.");
                //await next();
                return;
            }

            if (idValue is not int id || id < 0)
            {
                context.Result = new BadRequestObjectResult($"Invalid ID provided for {_idParameterName}.");
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<CiclilavariziaDevContext>();

            bool exists = await dbContext.Products
                .AsNoTracking()
                .AnyAsync(p => p.ProductID == id, context.HttpContext.RequestAborted);

            if (!exists)
            {
                context.Result = new NotFoundResult();
                return;
            }

            await next();
        }
    }
}
