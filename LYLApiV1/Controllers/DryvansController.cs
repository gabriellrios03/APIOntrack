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
    public class DryvansController : ControllerBase
    {
        private readonly string _connectionString;

        public DryvansController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetAllDryvans()
        {
            List<Dryvans> dryvans = new List<Dryvans>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id, name, plate, last_location FROM Dryvans", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dryvans.Add(new Dryvans
                            {
                                Id = reader.IsDBNull(0) ? null : reader.GetInt32(0),
                                Name = reader.GetString(1),
                                plate = reader.GetString(2),
                                last_location = reader.IsDBNull(3) ? null : reader.GetString(3)
                            });
                        }
                    }
                }
            }

            return Ok(new { success = true, message = "Dryvans retrieved successfully", data = dryvans });
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetDryvanById(int id)
        {
            Dryvans dryvan = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id, name, plate, last_location FROM Dryvans WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dryvan = new Dryvans
                            {
                                Id = reader.IsDBNull(0) ? null : reader.GetInt32(0),
                                Name = reader.GetString(1),
                                plate = reader.GetString(2),
                                last_location = reader.IsDBNull(3) ? null : reader.GetString(3)
                            };
                        }
                    }
                }
            }

            if (dryvan == null)
            {
                return NotFound(new { success = false, message = "Dryvan not found" });
            }

            return Ok(new { success = true, message = "Dryvan retrieved successfully", data = dryvan });
        }

        [HttpPost]
        [Authorize]
        public IActionResult CreateDryvan([FromBody] Dryvans dryvan)
        {
            if (dryvan == null)
            {
                return BadRequest(new { success = false, message = "Invalid dryvan data" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Dryvans (name, plate, last_location) OUTPUT INSERTED.id VALUES (@name, @plate, @last_location)", conn))
                {
                    cmd.Parameters.AddWithValue("@name", dryvan.Name);
                    cmd.Parameters.AddWithValue("@plate", dryvan.plate);
                    cmd.Parameters.AddWithValue("@last_location", (object)dryvan.last_location ?? DBNull.Value);

                    var newId = cmd.ExecuteScalar();
                    dryvan.Id = (int)newId;
                }
            }

            return CreatedAtAction(nameof(GetDryvanById), new { id = dryvan.Id }, new { success = true, message = "Dryvan created successfully", data = dryvan });
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult EditDryvan(int id, [FromBody] Dryvans dryvan)
        {
            if (dryvan == null)
            {
                return BadRequest(new { success = false, message = "Invalid dryvan data" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE Dryvans SET name = @name, plate = @plate, last_location = @last_location WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", dryvan.Name);
                    cmd.Parameters.AddWithValue("@plate", dryvan.plate);
                    cmd.Parameters.AddWithValue("@last_location", (object)dryvan.last_location ?? DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return NotFound(new { success = false, message = "Dryvan not found" });
                    }
                }
            }

            return Ok(new { success = true, message = "Dryvan edited successfully", data = dryvan });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteDryvan(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Dryvans WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return NotFound(new { success = false, message = "Dryvan not found" });
                    }
                }
            }

            return Ok(new { success = true, message = "Dryvan deleted successfully" });
        }
    }
}
