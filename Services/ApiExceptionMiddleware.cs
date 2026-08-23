using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace MetroStart.Services;

public sealed class ApiExceptionMiddleware(ILogger<ApiExceptionMiddleware> logger)
    : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API exception in {Function}", context.FunctionDefinition.Name);
            var httpContext = context.GetHttpContext();
            if (httpContext is null)
            {
                throw;
            }

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "The request could not be completed.",
            });
        }
    }
}
