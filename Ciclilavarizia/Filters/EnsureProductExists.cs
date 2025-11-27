using Ciclilavarizia.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Filters
{
    public class EnsureProductExists: Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // TODO: use proper Dependency Ijection and not this anti-pattern, what how to do it on youtube brotherrrrr susu froza
            int? id = (int?)context.ActionArguments["id"];
            CancellationToken cancellationToken = (CancellationToken)context.ActionArguments["cancellationToken"];
            var _db =  context.HttpContext.RequestServices.GetRequiredService<CiclilavariziaDevContext>();

            var result = await _db.Products
                .AsNoTracking()
                .Where(p => p.ProductID == id)
                .FirstOrDefaultAsync(cancellationToken);
            if (result == null)
            {
                context.Result = new NotFoundResult();
            }
            else
            {
                ActionExecutedContext executedContext = await next();
            }
        }
    }
}
