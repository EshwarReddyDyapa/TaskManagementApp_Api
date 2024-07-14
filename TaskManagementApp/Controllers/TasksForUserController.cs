using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Text.Json;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksForUserController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public TasksForUserController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("GetAllTasks")]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await dbContext.TasksForUsers.ToListAsync();
            return Ok(tasks);
        }

        [HttpPost("CreateTask")]
        public async Task<IActionResult> CreateNote(AddTasksForUserDto tasksForUserDto)
        {
            var TaskEntity = new TasksForUser()
            {
                Title = tasksForUserDto.Title,
                Description = tasksForUserDto.Description,
                DueDate = tasksForUserDto.DueDate,
                IsCompleted = tasksForUserDto.IsCompleted,
                EmployeeId = tasksForUserDto.EmployeeId
            };

            await dbContext.TasksForUsers.AddAsync(TaskEntity);
            await dbContext.SaveChangesAsync();

            return Ok(TaskEntity);
        }

        [HttpGet("GetTaskById/{id:int}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var userTask = await dbContext.TasksForUsers.FindAsync(id);
            if (userTask == null)
            {
                return NotFound(JsonSerializer.Serialize("NotFound"));
            }
            return Ok(userTask);
        }

        [HttpPut("UpdateTask/{id:int}")]
        public async Task<IActionResult> UpdateTask(int id, AddTasksForUserDto tasksForUserDto)
        {
            var userTask = dbContext.TasksForUsers.Find(id);
            if (userTask == null)
                return NotFound(JsonSerializer.Serialize("No Note found"));

            userTask.Title = tasksForUserDto.Title;
            userTask.Description = tasksForUserDto.Description;
            userTask.DueDate = tasksForUserDto.DueDate;
            userTask.IsCompleted = tasksForUserDto.IsCompleted;
            userTask.EmployeeId = tasksForUserDto.EmployeeId;

            await dbContext.SaveChangesAsync();

            return Ok(userTask);
        }

        [HttpDelete("DeleteTask/{id:int}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var usertask = await dbContext.TasksForUsers.FindAsync(id);
            if (usertask == null)
                return BadRequest(JsonSerializer.Serialize("No Task Found"));

            dbContext.TasksForUsers.Remove(usertask);
            await dbContext.SaveChangesAsync();

            return Ok(JsonSerializer.Serialize("User Task Deleted"));
        }
    }
}
