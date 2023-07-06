using Chat.BL.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

public class RequestLoggingMiddleware
{
    public class CustomRequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomRequestLoggingMiddleware> _logger;
        private readonly List<string> _logMessages;
        List<LogResponse> lstLogResponses;

        public CustomRequestLoggingMiddleware(RequestDelegate next, ILogger<CustomRequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _logMessages = new List<string>();
            lstLogResponses = new List<LogResponse>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Enable buffering to capture the request body
            context.Request.EnableBuffering();
            LogResponse logResponse = new LogResponse();

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            logResponse.IPOfCaller = ipAddress;
            logResponse.UserName = "";

            DateTime dateTime = DateTime.Now;
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
            logResponse.TimeOfCall = dateTimeOffset.ToUnixTimeSeconds();

            // Log the request information
            var logMessage = $"Request: {context.Request.Method} {context.Request.Path} {context.Request.QueryString}";
            _logMessages.Add(logMessage);

            logResponse.Method = context.Request.Method;
            logResponse.Path = context.Request.Path;
            logResponse.QueryString = context.Request.QueryString;

            // Read and log the request body if present
            string requestBody = await GetRequestBodyAsync(context.Request);

            logResponse.RequestBody = requestBody;

            if (!string.IsNullOrEmpty(requestBody))
            {
                var requestBodyLog = $"Request Body: {requestBody}";
                _logMessages.Add(requestBodyLog);
            }

            // Store the values in the HttpContext
            lstLogResponses.Add(logResponse);
            context.Items["logMessages"] = _logMessages;
            context.Items["lstLogResponses"] = lstLogResponses;

            // Call the next middleware in the pipeline
            await _next(context);
        }

        private async Task<dynamic> GetRequestBodyAsync(HttpRequest request)
        {

            // Ensure the request body can be read multiple times
            request.EnableBuffering();

            // Read the request body stream
            using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var requestBody = await reader.ReadToEndAsync();

            // Reset the request body position for subsequent middleware/components
            request.Body.Position = 0;

            return requestBody;
        }
    }
}
      



