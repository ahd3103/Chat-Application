using Microsoft.AspNetCore.Builder;
using static RequestLoggingMiddleware;

namespace Chat.BL.Helper
{
    public static class RequestLoggingMiddlewareExtension
    {
        public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomRequestLoggingMiddleware>();
        }
    }
}
