using Chat.DL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Chat.PL.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/log")]
    public class LogController : ControllerBase
    {
        private readonly IConfiguration _configuration;


        public LogController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetLogs(DateTime? startTime = null, DateTime? endTime = null)
        {
            // Validate request parameters
            if (startTime == null)
                startTime = DateTime.UtcNow.AddMinutes(-5);

            if (endTime == null)
                endTime = DateTime.UtcNow;

            if (startTime >= endTime)
                return BadRequest("Invalid request parameters. StartTime must be before EndTime.");

            // Fetch logs from PostgreSQL
            var logs = FetchLogsFromPostgreSQL(startTime.Value, endTime.Value);

            if (logs == null || logs.Count == 0)
                return NotFound("No logs found.");

            return Ok(logs);
        }

        //private List<LogEntry> FetchLogsFromPostgreSQL(DateTime startTime, DateTime endTime)
        //{
        //    var connectionString = _configuration.GetConnectionString("DefaultConnection");
        //    //var connectionString = _configuration["\"ConnectionStrings\": {\r\n    \"DefaultConnection\": \"User ID=postgres;Password=Ahd@3103;Host=localhost;Port=5432;Database=ChatApplicationDb;Pooling=true;\""];
        //    var query = "SELECT * FROM Logs WHERE raise_date >= @StartTime AND raise_date <= @EndTime";

        //    using (var connection = new NpgsqlConnection(connectionString))
        //    {
        //        connection.Open();

        //        using (var command = new NpgsqlCommand(query, connection))
        //        {
        //            command.Parameters.AddWithValue("StartTime", startTime);
        //            command.Parameters.AddWithValue("EndTime", endTime);

        //            using (var reader = command.ExecuteReader())
        //            {
        //                var logs = new List<LogEntry>();

        //                while (reader.Read())
        //                {
        //                    var logEntry = new LogEntry
        //                    {
        //                        Message = reader.GetString(reader.GetOrdinal("message")),
        //                        //Level = reader.GetString(reader.GetOrdinal("level")),
        //                        // Map other log properties accordingly
        //                    };

        //                    logs.Add(logEntry);
        //                }

        //                return logs;
        //            }
        //        }
        //    }
        //}

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
                                Message = reader.GetString(reader.GetOrdinal("Message")),
                                Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp")),
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

