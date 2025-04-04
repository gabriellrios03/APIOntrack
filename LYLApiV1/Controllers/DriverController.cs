using LYLApiV1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace LYLApiV1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly string _connectionString;

        public DriversController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/drivers
        [HttpGet]
        [Authorize]
        public IActionResult GetAllDrivers()
        {
            List<Drivers> drivers = new List<Drivers>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id, name, lastname, status_id, rfc FROM Drivers", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            drivers.Add(new Drivers
                            {
                                Id = reader.IsDBNull(0) ? null : reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Lastname = reader.GetString(2),
                                Status_Id = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                                Rfc = reader.GetString(4)
                            });
                        }
                    }
                }
            }

            return Ok(new { success = true, message = "Drivers retrieved successfully", data = drivers });
        }

        // GET: api/drivers/{id}
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetDriverById(int id)
        {
            Drivers driver = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id, name, lastname, status_id, rfc FROM Drivers WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            driver = new Drivers
                            {
                                Id = reader.IsDBNull(0) ? null : reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Lastname = reader.GetString(2),
                                Status_Id = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                                Rfc = reader.GetString(4)
                            };
                        }
                    }
                }
            }

            if (driver == null)
            {
                return NotFound(new { success = false, message = "Driver not found" });
            }

            return Ok(new { success = true, message = "Driver retrieved successfully", data = driver });
        }

        // POST: api/drivers
        [HttpPost]
        [Authorize]
        public IActionResult CreateDriver([FromBody] Drivers driver)
        {
            if (driver == null)
            {
                return BadRequest(new { success = false, message = "Invalid driver data" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Drivers (name, lastname, status_id, rfc) OUTPUT INSERTED.id VALUES (@name, @lastname, @status_id, @rfc)", conn))
                {
                    cmd.Parameters.AddWithValue("@name", driver.Name);
                    cmd.Parameters.AddWithValue("@lastname", driver.Lastname);
                    cmd.Parameters.AddWithValue("@status_id", (object)driver.Status_Id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@rfc", driver.Rfc);

                    var newId = cmd.ExecuteScalar();
                    driver.Id = (int)newId;
                }
            }

            return CreatedAtAction(nameof(GetDriverById), new { id = driver.Id }, new { success = true, message = "Driver created successfully", data = driver });
        }

        // PUT: api/drivers/{id}
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult EditDriver(int id, [FromBody] Drivers driver)
        {
            if (driver == null)
            {
                return BadRequest(new { success = false, message = "Invalid driver data" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE Drivers SET name = @name, lastname = @lastname, status_id = @status_id, rfc = @rfc WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", driver.Name);
                    cmd.Parameters.AddWithValue("@lastname", driver.Lastname);
                    cmd.Parameters.AddWithValue("@status_id", (object)driver.Status_Id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@rfc", driver.Rfc);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return NotFound(new { success = false, message = "Driver not found" });
                    }
                }
            }

            return Ok(new { success = true, message = "Driver edited successfully", data = driver });
        }

        // DELETE: api/drivers/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteDriver(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Drivers WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return NotFound(new { success = false, message = "Driver not found" });
                    }
                }
            }

            return Ok(new { success = true, message = "Driver deleted successfully" });
        }
    }
}
