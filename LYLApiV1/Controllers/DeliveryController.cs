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
    public class DeliveryController : ControllerBase
    {
        private readonly string _connectionString;

        public DeliveryController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
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
                // Agregar un log para ver el valor del header_id
                Console.WriteLine($"Valor de header_id recibido: {delivery.Id}");

                // Verificar que el header_id exista en DeliveryHdrTbl antes de llamar al SP
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM DeliveryHdrTbl WHERE id = @header_id", conn))
                    {
                        cmd.Parameters.AddWithValue("@header_id", delivery.Id);

                        var count = (int)cmd.ExecuteScalar();
                        if (count == 0)
                        {
                            return BadRequest(new { success = false, message = "El header_id proporcionado no existe en DeliveryHdrTbl" });
                        }
                    }
                }

                // Procedimiento almacenado para crear un delivery
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
