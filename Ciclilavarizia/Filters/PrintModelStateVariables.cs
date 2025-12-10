using Microsoft.AspNetCore.Mvc.Filters;

namespace Ciclilavarizia.Filters
{
    public class PrintModelStateVariables: Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Console.WriteLine("--------------------PrintModelStateVariables--------------------");

            foreach(KeyValuePair<string, object> kvp in context.ActionArguments)
            {
                Console.WriteLine($"\tKey: {kvp.Key} - Value: {kvp.Value}.s");
            }

            Console.WriteLine("--------------------PrintModelStateVariables--------------------");
            next();
        }
    }
}
