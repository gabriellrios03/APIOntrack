using LYLApiV1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LYLApiV1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
        }

        // 🔹 REGISTRO DE USUARIO
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return BadRequest(new { success = false, message = "Invalid user data" });
            }

            // Hasheamos la contraseña antes de almacenarla
            user.PasswordHash = HashPassword(user.PasswordHash);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Users (FirstName, LastName, Email, Password) VALUES (@FirstName, @LastName, @Email, @Password)", conn))
                {
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", user.PasswordHash);  // Se almacena la contraseña hasheada

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        // Manejar el error si el correo ya está registrado
                        return Conflict(new { success = false, message = "Email already registered." });
                    }
                }
            }

            // Generar token JWT después del registro
            string token = GenerateJwtToken(user);

            var result = new
            {
                success = true,
                message = "User registered successfully",
                data = new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    token
                }
            };

            return Ok(result);
        }

        // 🔹 LOGIN (AUTENTICACIÓN)
        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginRequest)
        {
            if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.PasswordHash))
            {
                return BadRequest(new { success = false, message = "Invalid login data" });
            }

            User user = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Id, FirstName, LastName, Email, Password FROM Users WHERE Email = @Email", conn))
                {
                    cmd.Parameters.AddWithValue("@Email", loginRequest.Email);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Email = reader.GetString(3),
                                PasswordHash = reader.GetString(4)  // La contraseña está hasheada en la base de datos
                            };
                        }
                    }
                }
            }

            if (user == null || !VerifyPassword(loginRequest.PasswordHash, user.PasswordHash))
            {
                return Unauthorized(new { success = false, message = "Invalid credentials" });
            }

            // Generar JWT
            string token = GenerateJwtToken(user);

            var result = new
            {
                success = true,
                message = "Login successful",
                data = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    token
                }
            };

            return Ok(result);
        }

        // 🔹 FUNCIÓN PARA HASHEAR LA CONTRASEÑA (SHA512)
        private string HashPassword(string password)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);  // Se retorna el hash en base64
            }
        }

        // 🔹 VERIFICAR CONTRASEÑA HASHEADA
        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Hasheamos la contraseña ingresada y la comparamos con el hash almacenado
            return HashPassword(enteredPassword) == storedHash;
        }

        // 🔹 GENERAR TOKEN JWT
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
