using LYLApiV1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace LYLApiV1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveriesHDRController : ControllerBase
    {
        private readonly string _connectionString;

        public DeliveriesHDRController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // POST: api/DeliveriesHDR
        [HttpPost]
        [Authorize]
        public IActionResult CreateDeliveryHDR([FromBody] DeliveriesHdr delivery)
        {
            if (delivery == null)
            {
                return BadRequest(new { success = false, message = "Invalid delivery data" });
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO DeliveryHdrTbl 
                    (customer_id, truck_id, route_id, appointment_date, status_id) 
                OUTPUT INSERTED.id 
                VALUES 
                    (@customer_id, @truck_id, @route_id, @appointment_date, @status_id)", conn))
                    {
                        cmd.Parameters.AddWithValue("@customer_id", delivery.Customer_id);
                        cmd.Parameters.AddWithValue("@truck_id", delivery.Truck_id);
                        cmd.Parameters.AddWithValue("@route_id", delivery.Route_id);
                        cmd.Parameters.AddWithValue("@appointment_date", delivery.Appointment_date);
                        cmd.Parameters.AddWithValue("@status_id", delivery.Status_id);

                        var newId = cmd.ExecuteScalar();
                        delivery.Id = Convert.ToInt32(newId);
                    }
                }

                return CreatedAtAction(nameof(CreateDeliveryHDR), new { id = delivery.Id }, new
                {
                    success = true,
                    message = "Delivery created successfully",
                    data = delivery
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = $"Error creating delivery: {ex.Message}"
                });
            }
        }


        // GET: api/DeliveriesHDR
        [HttpGet]
        [Authorize]
        public IActionResult GetAllDeliveriesHDR([FromQuery] int? status_id = null)
        {
            try
            {
                var deliveries = new List<Dictionary<string, object>>();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    string sql = "SELECT * FROM PreDeliveries";
                    if (status_id.HasValue)
                    {
                        sql += " WHERE status_id = @status_id";
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (status_id.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@status_id", status_id.Value);
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var delivery = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    delivery[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }

                                deliveries.Add(delivery);
                            }
                        }
                    }
                }

                return Ok(new { success = true, data = deliveries });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = $"Error retrieving deliveries: {ex.Message}"
                });
            }
        }

        // PUT: api/DeliveriesHDR/Cancel/{id}
        [HttpPut("Cancel/{id}")]
        [Authorize]
        public IActionResult CancelDeliveryHDR(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Check if the record exists
                    using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(1) FROM DeliveryHdrTbl WHERE id = @id", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@id", id);
                        var exists = (int)checkCmd.ExecuteScalar();

                        if (exists == 0)
                        {
                            return NotFound(new { success = false, message = "Delivery not found" });
                        }
                    }

                    // Update the status to 2 (Cancelled)
                    using (SqlCommand cmd = new SqlCommand("UPDATE DeliveryHdrTbl SET status_id = 2 WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "Delivery cancelled successfully" });
                        }

                        return StatusCode(StatusCodes.Status500InternalServerError, new
                        {
                            success = false,
                            message = "Failed to cancel delivery"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = $"Error cancelling delivery: {ex.Message}"
                });
            }
        }
    }
}
