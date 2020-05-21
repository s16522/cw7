using System;
using System.Data.SqlClient;
using System.Transactions;
using cwiczenie3.DAL;
using cwiczenie3.DTO;
using cwiczenie3.Models;
using cwiczenie3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cwiczenie3.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    [Authorize(Roles = "employee")]
    public class EnrollmentsController : ControllerBase
    {
        private IStudentsDbService _service;

        public EnrollmentsController(IStudentsDbService service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult EnrollStudent(PostStudentRequest student)
        {
            var response = _service.PostStudent(student);

            switch (response)
            {
                case "Not Found":
                    return NotFound();
                case "BadRequest":
                    return BadRequest();
                default:
                    return Ok(response);
            }
        }

        [HttpPost]
        [Route("api/enrollments/promotions")]
        public IActionResult PromoteStudent(PromoteStudentRequest request)
        {   
            var response = _service.PromoteStudents(request);

            switch (response)
            {
                case "Not Found":
                    return NotFound();
                default:
                    return Ok(response);
            }

        }

    }
}