using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public EmployeeController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("GetAllEmployees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await dbContext.Employees.ToListAsync();
            return Ok(employees);
        }

        [HttpPost("CreateEmployee")]
        public async Task<IActionResult> CreateEmployee(AddEmployeeDto addEmployeeDto)
        {
            var empEntity = new Employee()
            {
                Name = addEmployeeDto.Name,
                ManagerId = addEmployeeDto.ManagerId,
                Position = addEmployeeDto.Position
            };

            await dbContext.AddAsync(empEntity);
            await dbContext.SaveChangesAsync();

            return Ok(empEntity);
        }

        [HttpGet("GetEmployeeById/{id:int}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await dbContext.Employees.FindAsync(id);

            if(employee == null)
                return NotFound(JsonSerializer.Serialize("Employee does not exist"));
            return Ok(employee);
        }

        [HttpPut("UpdateEmployeeById/{id:int}")]
        public async Task<IActionResult> UpdateEmployeeById(int id, AddEmployeeDto addEmployeeDto)
        {
            var emp = await dbContext.Employees.FindAsync(id);
            if (emp == null)
                return NotFound(JsonSerializer.Serialize("Not Found"));
            
            emp.Name = addEmployeeDto.Name;
            emp.Position = addEmployeeDto.Position;
            emp.ManagerId = addEmployeeDto.ManagerId;

            await dbContext.SaveChangesAsync();

            return Ok(emp);
        }

        [HttpDelete("DeleteEmployee/{id:int}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var emp = await dbContext.Employees.FindAsync(id);
            if (emp == null)
                return NotFound(JsonSerializer.Serialize("Employee Doesn't Exist"));

            dbContext.Employees.Remove(emp);
            await dbContext.SaveChangesAsync();

            return Ok(JsonSerializer.Serialize("Employee Data is deleted"));
        }

    }
}
