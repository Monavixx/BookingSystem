using BookingSystem.Api.Extensions;

namespace BookingSystem.Api.Middlewares;

public class LogContextMiddleware(RequestDelegate next, ILogger<LogContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.GetUserIdOrDefault();
        using (logger.BeginScope(new Dictionary<string, object?>
               {
                   ["UserId"] = userId,
               }))
        {
            await next(context);
        }
    }
}