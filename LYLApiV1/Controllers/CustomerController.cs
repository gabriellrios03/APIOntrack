using LYLApiV1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;  // Agrega este using
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace LYLApiV1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly string _connectionString;

        public CustomerController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection"); // Assuming "DefaultConnection" in appsettings.json
        }

        // GET: api/Customer
        // Este método está protegido por JWT. Solo usuarios autenticados pueden acceder.
        [HttpGet]
        [Authorize]  // Agrega este atributo para proteger la ruta
        public IActionResult GetAllCustomers()
        {
            List<Customer> customers = new List<Customer>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id, name, rfc FROM Customers", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new Customer
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Rfc = reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return Ok(new { success = true, message = "Customers retrieved successfully", data = customers });
        }

        // GET: api/Customer/{id}
        [HttpGet("{id}")]
        [Authorize]  // Agrega este atributo para proteger la ruta
        public IActionResult GetCustomerById(int id)
        {
            Customer customer = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id, name, rfc FROM Customers WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = new Customer
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Rfc = reader.GetString(2)
                            };
                        }
                    }
                }
            }

            if (customer == null)
            {
                return NotFound(new { success = false, message = "Customer not found" });
            }

            return Ok(new { success = true, message = "Customer retrieved successfully", data = customer });
        }

        // POST: api/Customer
        [HttpPost]
        [Authorize]  // Agrega este atributo para proteger la ruta
        public IActionResult CreateCustomer([FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest(new { success = false, message = "Invalid customer data" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Customers (name, rfc) OUTPUT INSERTED.id VALUES (@name, @rfc)", conn))
                {
                    cmd.Parameters.AddWithValue("@name", customer.Name);
                    cmd.Parameters.AddWithValue("@rfc", customer.Rfc);

                    var newId = cmd.ExecuteScalar();
                    customer.Id = (int)newId;
                }
            }

            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, new { success = true, message = "Customer created successfully", data = customer });
        }

        // PUT: api/Customer/{id}
        [HttpPut("{id}")]
        [Authorize]  // Agrega este atributo para proteger la ruta
        public IActionResult EditCustomer(int id, [FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest(new { success = false, message = "Invalid customer data" });
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE Customers SET name = @name, rfc = @rfc WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", customer.Name);
                    cmd.Parameters.AddWithValue("@rfc", customer.Rfc);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return NotFound(new { success = false, message = "Customer not found" });
                    }
                }
            }

            return Ok(new { success = true, message = "Customer edit successfully", data = customer });
        }

        // DELETE: api/Customer/{id}
        [HttpDelete("{id}")]
        [Authorize]  // Agrega este atributo para proteger la ruta
        public IActionResult DeleteCustomer(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Customers WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return NotFound(new { success = false, message = "Customer not found" });
                    }
                }
            }

            return Ok(new { success = true, message = "Customer deleted successfully" });
        }
    }
}
