using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZktecoController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ZktecoController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("insert-log")]
        public IActionResult InsertLog([FromBody] AttendanceDto data)
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new SqlConnection(connStr);
                conn.Open();

                var cmd = new SqlCommand("INSERT INTO AttendanceLogs (UserId, Name, Timestamp, Status) VALUES (@UserId, @Name, @Timestamp, @Status)", conn);
                cmd.Parameters.AddWithValue("@UserId", data.UserId);
                cmd.Parameters.AddWithValue("@Name", data.Name ?? "Unknown");
                cmd.Parameters.AddWithValue("@Timestamp", data.Timestamp);
                cmd.Parameters.AddWithValue("@Status", data.Status);

                cmd.ExecuteNonQuery();
                return Ok("✅ Data inserted");
            }
            catch (Exception ex)
            {
                return BadRequest("❌ Error: " + ex.Message);
            }
        }

        [HttpGet("get-logs/{name}")]
        public IActionResult GetLogsByName(string name)
        {
            try
            {
                var logs = new List<AttendanceDto>();
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new SqlConnection(connStr);
                conn.Open();

                var cmd = new SqlCommand("SELECT UserId, Name, Timestamp, Status FROM AttendanceLogs WHERE Name = @Name", conn);
                cmd.Parameters.AddWithValue("@Name", name);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    DateTime timestamp;

                    // Safe handling if Timestamp is mistakenly stored as a string
                    if (reader["Timestamp"] is DateTime dt)
                    {
                        timestamp = dt;
                    }
                    else if (!DateTime.TryParse(reader["Timestamp"].ToString(), out timestamp))
                    {
                        timestamp = DateTime.MinValue; // default or log warning
                    }

                    logs.Add(new AttendanceDto
                    {
                        UserId = reader["UserId"].ToString(),
                        Name = reader["Name"].ToString(),
                        Timestamp = timestamp,
                        Status = Convert.ToInt32(reader["Status"])
                    });
                }

                if (logs.Count == 0)
                {
                    return NotFound(new { Message = $"No attendance records found for name: {name}" });
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("get-all-logs")]
        public IActionResult GetAllLogs()
        {
            try
            {
                var logs = new List<AttendanceDto>();
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new SqlConnection(connStr);
                conn.Open();

                var cmd = new SqlCommand("SELECT UserId, Name, Timestamp, Status FROM AttendanceLogs", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    DateTime timestamp;

                    if (reader["Timestamp"] is DateTime dt)
                    {
                        timestamp = dt;
                    }
                    else if (!DateTime.TryParse(reader["Timestamp"].ToString(), out timestamp))
                    {
                        timestamp = DateTime.MinValue;
                    }

                    logs.Add(new AttendanceDto
                    {
                        UserId = reader["UserId"].ToString(),
                        Name = reader["Name"].ToString(),
                        Timestamp = timestamp,
                        Status = Convert.ToInt32(reader["Status"])
                    });
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }
    }

    public class AttendanceDto
    {
        public string UserId { get; set; } = "";
        public string Name { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public int Status { get; set; }
    }
}
