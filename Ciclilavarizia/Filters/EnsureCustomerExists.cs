using Ciclilavarizia.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Filters
{
    public class EnsureCustomerExists : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            int? id = (int?)context.ActionArguments["customerId"] ?? null;
            if (id == null || id < 0)
            {
                context.Result = new BadRequestObjectResult($"Invalid ID provided for customerId.");
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<CiclilavariziaDevContext>();
            bool exists = await db.Customers
                .AsNoTracking()
                .AnyAsync(c => c.CustomerID == id, context.HttpContext.RequestAborted);

            if (!exists)
            {
                context.Result = new NotFoundResult();
                return;
            }

            await next();
        }
    }
}
