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
    public class RoutesController : ControllerBase
    {
        private readonly string _connectionString;

        public RoutesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Obtener todos los Routes
        [HttpGet]
        [Authorize]
        public IActionResult GetAllRoutes()
        {
            List<Routes> routes = new List<Routes>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Id, Customer_Id, Origin, Destination, Distance_Km, Avg_fuel_consumption, Customer_price, Driver_profit FROM Routes", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            routes.Add(new Routes
                            {
                                Id = reader.GetInt32(0),
                                Customer_Id = reader.GetInt32(1),
                                Origin = reader.GetString(2),
                                Destination = reader.GetString(3),
                                Distance_Km = reader.GetDecimal(4),
                                Avg_fuel_consumption = reader.GetDecimal(5),
                                Customer_price = reader.GetDecimal(6),
                                Driver_profit = reader.GetDecimal(7)
                            });
                        }
                    }
                }
            }

            return Ok(new { success = true, message = "Routes retrieved successfully", data = routes });
        }

        // Obtener un Route por su ID
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetRouteById(int id)
        {
            Routes route = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Id, Customer_Id, Origin, Destination, Distance_Km, Avg_fuel_consumption, Customer_price, Driver_profit FROM Routes WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            route = new Routes
                            {
                                Id = reader.GetInt32(0),
                                Customer_Id = reader.GetInt32(1),
                                Origin = reader.GetString(2),
                                Destination = reader.GetString(3),
                                Distance_Km = reader.GetDecimal(4),
                                Avg_fuel_consumption = reader.GetDecimal(5),
                                Customer_price = reader.GetDecimal(6),
                                Driver_profit = reader.GetDecimal(7)
                            };
                        }
                    }
                }
            }

            if (route == null)
            {
                return NotFound(new { success = false, message = "Route not found" });
            }

            return Ok(new { success = true, message = "Route retrieved successfully", data = route });
        }

        // Crear un nuevo Route
        [HttpPost]
        [Authorize]
        public IActionResult CreateRoute([FromBody] Routes route)
        {
            if (route == null)
            {
                return BadRequest(new { success = false, message = "Invalid route data" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Routes (Customer_Id, Origin, Destination, Distance_Km, Avg_fuel_consumption, Customer_price, Driver_profit) OUTPUT INSERTED.Id VALUES (@Customer_Id, @Origin, @Destination, @Distance_Km, @Avg_fuel_consumption, @Customer_price, @Driver_profit)", conn))
                {
                    cmd.Parameters.AddWithValue("@Customer_Id", route.Customer_Id);
                    cmd.Parameters.AddWithValue("@Origin", route.Origin);
                    cmd.Parameters.AddWithValue("@Destination", route.Destination);
                    cmd.Parameters.AddWithValue("@Distance_Km", route.Distance_Km);
                    cmd.Parameters.AddWithValue("@Avg_fuel_consumption", route.Avg_fuel_consumption);
                    cmd.Parameters.AddWithValue("@Customer_price", route.Customer_price);
                    cmd.Parameters.AddWithValue("@Driver_profit", route.Driver_profit);

                    var newId = cmd.ExecuteScalar();
                    route.Id = (int)newId;
                }
            }

            return CreatedAtAction(nameof(GetRouteById), new { id = route.Id }, new { success = true, message = "Route created successfully", data = route });
        }

        // Editar un Route existente
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult EditRoute(int id, [FromBody] Routes route)
        {
            if (route == null)
            {
                return BadRequest(new { success = false, message = "Invalid route data" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE Routes SET Customer_Id = @Customer_Id, Origin = @Origin, Destination = @Destination, Distance_Km = @Distance_Km, Avg_fuel_consumption = @Avg_fuel_consumption, Customer_price = @Customer_price, Driver_profit = @Driver_profit WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@Customer_Id", route.Customer_Id);
                    cmd.Parameters.AddWithValue("@Origin", route.Origin);
                    cmd.Parameters.AddWithValue("@Destination", route.Destination);
                    cmd.Parameters.AddWithValue("@Distance_Km", route.Distance_Km);
                    cmd.Parameters.AddWithValue("@Avg_fuel_consumption", route.Avg_fuel_consumption);
                    cmd.Parameters.AddWithValue("@Customer_price", route.Customer_price);
                    cmd.Parameters.AddWithValue("@Driver_profit", route.Driver_profit);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return NotFound(new { success = false, message = "Route not found" });
                    }
                }
            }

            return Ok(new { success = true, message = "Route edited successfully", data = route });
        }

        // Eliminar un Route
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteRoute(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Routes WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return NotFound(new { success = false, message = "Route not found" });
                    }
                }
            }

            return Ok(new { success = true, message = "Route deleted successfully" });
        }
    }
}
