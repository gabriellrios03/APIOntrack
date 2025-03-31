using LYLApiV1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace LYLApiV1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TruckController : ControllerBase
    {
        private readonly string _connectionString;

        public TruckController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Trucks
        [HttpGet]
        [Authorize]
        public IActionResult GetAllTrucks()
        {
            List<Trucks> trucks = new List<Trucks>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id, name, plate FROM Trucks", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            trucks.Add(new Trucks
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Plate = reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return Ok(new { success = true, message = "Trucks retrieved successfully", data = trucks });
        }

        // GET: api/Trucks/{id}
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetTruckById(int id)
        {
            Trucks truck = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id, name, plate FROM Trucks WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            truck = new Trucks
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Plate = reader.GetString(2)
                            };
                        }
                    }
                }
            }

            if (truck == null)
            {
                return NotFound(new { success = false, message = "Trucks not found" });
            }

            return Ok(new { success = true, message = "Trucks retrieved successfully", data = truck });
        }

        // POST: api/Trucks
        [HttpPost]
        [Authorize]
        public IActionResult CreateTruck([FromBody] Trucks truck)
        {
            if (truck == null)
            {
                return BadRequest(new { success = false, message = "Invalid truck data" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Trucks (name, plate) OUTPUT INSERTED.id VALUES (@name, @plate)", conn))
                {
                    cmd.Parameters.AddWithValue("@name", truck.Name);
                    cmd.Parameters.AddWithValue("@plate", truck.Plate);

                    var newId = cmd.ExecuteScalar();
                    truck.Id = (int)newId;
                }
            }

            return CreatedAtAction(nameof(GetTruckById), new { id = truck.Id }, new { success = true, message = "Trucks created successfully", data = truck });
        }

        // PUT: api/Trucks/{id}
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult EditTruck(int id, [FromBody] Trucks truck)
        {
            if (truck == null)
            {
                return BadRequest(new { success = false, message = "Invalid truck data" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE Trucks SET name = @name, plate = @plate WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", truck.Name);
                    cmd.Parameters.AddWithValue("@plate", truck.Plate);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return NotFound(new { success = false, message = "Trucks not found" });
                    }
                }
            }

            return Ok(new { success = true, message = "Trucks updated successfully", data = truck });
        }

        // DELETE: api/Trucks/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteTruck(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Trucks WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return NotFound(new { success = false, message = "Trucks not found" });
                    }
                }
            }

            return Ok(new { success = true, message = "Trucks deleted successfully" });
        }
    }
}