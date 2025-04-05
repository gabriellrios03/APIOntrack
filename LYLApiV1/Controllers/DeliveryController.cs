using LYLApiV1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace LYLApiV1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryHDRController : ControllerBase
    {
        private readonly string _connectionString;

        public DeliveryHDRController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // POST: api/Delivery
        [HttpPost]
        [Authorize]
        public IActionResult CreateDeliveryHDR([FromBody] DeliveriesHdr delivery)
        {
            if (delivery == null)
            {
                return BadRequest(new { success = false, message = "Invalid delivery data" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO DeliveryHdrTbl (customer_id, truck_id, route_id, appointment_date, created_at, status_id) 
                                                         OUTPUT INSERTED.id 
                                                         VALUES (@customer_id, @truck_id, @route_id, @appointment_date, GETDATE(), @status_id)", conn))
                {
                    cmd.Parameters.AddWithValue("@customer_id", delivery.Customer_id);
                    cmd.Parameters.AddWithValue("@truck_id", delivery.Truck_id);
                    cmd.Parameters.AddWithValue("@route_id", delivery.Route_id);
                    cmd.Parameters.AddWithValue("@appointment_date", delivery.Appoinment_date);
                    cmd.Parameters.AddWithValue("@status_id", delivery.Status_id);

                    var newId = cmd.ExecuteScalar();
                    delivery.Id = (int)newId;
                }
            }

            return CreatedAtAction(nameof(CreateDeliveryHDR), new { id = delivery.Id }, new { success = true, message = "Delivery created successfully", data = delivery });
        }


        // GET: api/Delivery
        [HttpGet]
        [Authorize]
        public IActionResult GetAllDeliveriesHDR([FromQuery] int? status_id = null)
        {
            try
            {
                List<Dictionary<string, object>> deliveries = new List<Dictionary<string, object>>();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Construir la consulta SQL dinámicamente
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
                                    delivery.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));
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
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = $"Error retrieving deliveries: {ex.Message}" });
            }
        }

        // PUT: api/Delivery/Cancel/{id}
        [HttpPut("Cancel/{id}")]
        [Authorize]
        public IActionResult CancelDeliveryHDR(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Verificar si la entrega existe
                    using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(1) FROM DeliveryHdrTbl WHERE id = @id", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@id", id);
                        var exists = (int)checkCmd.ExecuteScalar();

                        if (exists == 0)
                        {
                            return NotFound(new { success = false, message = "Delivery not found" });
                        }
                    }

                    // Actualizar el estado a cancelado (status_id = 2)
                    using (SqlCommand cmd = new SqlCommand("UPDATE DeliveryHdrTbl SET status_id = 2 WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "Delivery cancelled successfully" });
                        }

                        return StatusCode(StatusCodes.Status500InternalServerError,
                            new { success = false, message = "Failed to cancel delivery" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = $"Error cancelling delivery: {ex.Message}" });
            }
        }

        // DELIVERIES
        // POST: api/Delivery
        [HttpPost]
        [Authorize]
        public IActionResult CreateDelivery([FromBody] Deliveries delivery)
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
                    using (SqlCommand cmd = new SqlCommand("CreateDelivery", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Agregar parámetros al procedimiento almacenado
                        cmd.Parameters.AddWithValue("@header_id", delivery.Id);
                        cmd.Parameters.AddWithValue("@dryvan_id", delivery.Dryvan_id);
                        cmd.Parameters.AddWithValue("@remision_number", delivery.Remision_number);
                        cmd.Parameters.AddWithValue("@documents_status", delivery.Documents_status);
                        cmd.Parameters.AddWithValue("@notes", delivery.Notes);

                        var newId = cmd.ExecuteScalar();

                        if (newId != null)
                        {
                            return CreatedAtAction(nameof(CreateDelivery), new { id = newId }, new { success = true, message = "Delivery created successfully", data = newId });
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Error creating delivery" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = $"Error creating delivery: {ex.Message}" });
            }
        }
        // GET: api/Delivery
        [HttpGet]
        [Authorize]
        public IActionResult GetAllDeliveries([FromQuery] int? status_id = null)
        {
            try
            {
                List<Dictionary<string, object>> deliveries = new List<Dictionary<string, object>>();

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
                                    delivery.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));
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
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = $"Error retrieving deliveries: {ex.Message}" });
            }
        }

        // GET: api/Delivery/{id}
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetDeliveryById(int id)
        {
            try
            {
                Dictionary<string, object> delivery = new Dictionary<string, object>();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    string sql = "SELECT * FROM PreDeliveries WHERE id = @id";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    delivery.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));
                                }
                            }
                        }
                    }
                }

                if (delivery.Count == 0)
                {
                    return NotFound(new { success = false, message = "Delivery not found" });
                }

                return Ok(new { success = true, data = delivery });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = $"Error retrieving delivery: {ex.Message}" });
            }
        }


        // PUT: api/Delivery/{id}
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult UpdateDelivery(int id, [FromBody] Deliveries delivery)
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

                    // Actualiza los datos de la entrega
                    using (SqlCommand cmd = new SqlCommand("UPDATE Deliverys SET remision_number = @remision_number, documents_status = @documents_status, notes = @notes WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@remision_number", delivery.Remision_number);
                        cmd.Parameters.AddWithValue("@documents_status", delivery.Documents_status);
                        cmd.Parameters.AddWithValue("@notes", delivery.Notes);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "Delivery updated successfully" });
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Failed to update delivery" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = $"Error updating delivery: {ex.Message}" });
            }
        }

    }
}
