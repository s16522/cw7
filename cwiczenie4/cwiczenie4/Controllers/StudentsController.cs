using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using cwiczenie3.DAL;
using cwiczenie3.DTO;
using cwiczenie3.Models;
using cwiczenie3.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace cwiczenie3.Controllers
{
    [ApiController]
    [Route("api/students")]
    
    public class StudentsController : ControllerBase
    {
        private readonly SqlServerDbService _dbService;
        private readonly IConfiguration _configuration;
        
        public StudentsController(SqlServerDbService dbService, IConfiguration configuration)
        {
            _dbService = dbService;
            _configuration = configuration;
        }

        
        [HttpGet]
        public IActionResult GetStudent()
        {
           return Ok(_dbService.GetStudents());
        }

        [HttpGet("{id}")]
        public IActionResult GetStudentById(string id)
        {
            return Ok(_dbService.GetStudent(id));
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
           // student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateStudent() {
            return Ok("Aktualizacja dokończona");
        }
        
        [HttpDelete("{id}")]
        public IActionResult DeleteStudent() {
            return Ok("Usuwanie ukończone");
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(LoginRequest request)
        {

            var claims = _dbService.Login(request); 
            return Authenticate(claims);
        }
        
        [HttpPost("refresh-token/{token}")]
        public IActionResult Login(string token)
        {
            var claims = _dbService.Login(token);
            return Authenticate(claims);
        }

        private IActionResult Authenticate(AuthenticationResult result)
        {
            if (result == null)
            {
                return Unauthorized();
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "Gakko",
                audience:"Students", 
                claims: result.Claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: creds
            );
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = result.RefreshToken
            });
        }
        
    }
}