﻿using Chat.DL.DbContexts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Chat.DL.Models;
using System.Text;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly ChatDbContext _dbContext;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, ChatDbContext dbContext)
    {
        _next = next;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Invoke(HttpContext context)
    {
        // Log request details
        string ipAddress = context.Connection.RemoteIpAddress.ToString();
        string requestPath = context.Request.Path;
        string requestBody = await GetRequestBody(context.Request);

        // Fetch username from auth token
        string username = await FetchUsernameFromToken(context.Request);

        // Log the request details
        LogRequest(ipAddress, requestPath, requestBody, username);

        // Call the next middleware
        await _next(context);
    }

    private async Task<string> GetRequestBody(HttpRequest request)
    {
        // Read the request body
        using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8))
        {
            return await reader.ReadToEndAsync();
        }
    }

    private async Task<string> FetchUsernameFromToken(HttpRequest request)
    {
        // Fetch the authentication token from the request headers
        string token = request.Headers["Authorization"];

        if (string.IsNullOrEmpty(token))
        {
            return string.Empty;
        }

        // TODO: Implement logic to extract and decode the username from the token
        // Example: Assuming the token format is "Bearer <token>", you can extract the token value using token.Split(' ')[1]
        // Then, decode the token and extract the username from it

        // Return the fetched username or an empty string if not found
        return string.Empty;
    }

    private void LogRequest(string ipAddress, string requestPath, string requestBody, string username)
    {
        // Log the request details to your preferred logging mechanism
        // You can use any logging framework such as Serilog, NLog, or log directly to a file or database

        string logMessage = $"IP: {ipAddress} | Request Path: {requestPath} | Request Body: {requestBody} | Username: {username} | Time: {DateTime.UtcNow}";

        // Example: Logging to the console
        _logger.LogInformation(logMessage);

        // Save the log to the Logs table using your preferred ORM or database access method
        var log = new Log
        {
            IpAddress = ipAddress,
            RequestPath = requestPath,
            RequestBody = requestBody,
            Username = username,
            //Time = DateTime.UtcNow
        };

        _dbContext.Logs.Add(log);
        _dbContext.SaveChanges();
    }
}