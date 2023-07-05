using Chat.DL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
//using Serilog;

namespace Chat.PL.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/log")]
    public class LogController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly RequestLoggingMiddleware _requestLoggingMiddleware;


        public LogController(IConfiguration configuration, ILogger<RequestLoggingMiddleware> logger, RequestLoggingMiddleware requestLoggingMiddleware)
        {
            _configuration = configuration;
            _logger = logger;
            _requestLoggingMiddleware = requestLoggingMiddleware;
        }
        [HttpGet]
        [Route("GetLogs")]
        public IActionResult GetLogs(DateTime startTime, DateTime endTime)
        {
            // Validate request parameters
            if (startTime == null)
                startTime = DateTime.UtcNow.AddMinutes(-5);

            if (endTime == null)
                endTime = DateTime.UtcNow;

            if (startTime >= endTime)
                return BadRequest("Invalid request parameters. StartTime must be before EndTime.");

            // Fetch logs from PostgreSQL
            var logs = FetchLogsFromPostgreSQL(startTime, endTime);

            if (logs == null || logs.Count == 0)
                return NotFound("No logs found.");

            return Ok(logs);
        }
        private List<Log> FetchLogsFromPostgreSQL(DateTime startTime, DateTime endTime)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var query = "SELECT * FROM Logs WHERE timestamp >= @StartTime AND timestamp <= @EndTime";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("StartTime", startTime);
                    command.Parameters.AddWithValue("EndTime", endTime);

                    using (var reader = command.ExecuteReader())
                    {
                        var logs = new List<Log>();

                        while (reader.Read())
                        {
                            var logEntry = new Log
                            {
                                Message = reader.GetString(reader.GetOrdinal("message")),
                                Level = reader.GetString(reader.GetOrdinal("level")),
                                // Map other log properties accordingly
                            };

                            logs.Add(logEntry);
                        }

                        return logs;
                    }
                }
            }
        }
    }
}

