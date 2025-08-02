using Microsoft.AspNetCore.Diagnostics;
using System.Text.RegularExpressions;

namespace AdPlatformsApi.Handlers
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "An unhandled exception occurred.");

            if (exception is ArgumentException argException)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new { error = argException.Message }, cancellationToken: CancellationToken.None);
            }
            else
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsJsonAsync(new { error = "Ooops! It wasn't me, but we will fix it ASAP." }, cancellationToken: CancellationToken.None);
            }

            return true;
        }
    }
}
