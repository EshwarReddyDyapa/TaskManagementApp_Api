using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskManagementController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IWebHostEnvironment _env;

        public TaskManagementController(ApplicationDbContext dbContext, IWebHostEnvironment env)
        {
            this.dbContext = dbContext;
            _env = env;
        }

        [HttpGet("GetTasksForUser/{EmpId:int}")]
        public async Task<IActionResult> GetTasksForUser(int EmpId)
        {
            var query = from t in dbContext.TasksForUsers
                        join e in dbContext.Employees on t.EmployeeId equals e.Id
                        where t.EmployeeId == EmpId
                        select new
                        {
                            Task = t,
                            Employee = e
                        };

            var result = await query.ToListAsync();

            var groupedResult = result
                .GroupBy(x => x.Employee)
                .Select(g => new
                {
                    Employee = g.Key,
                    Tasks = g.Select(x => x.Task).ToList()
                })
                .ToList();

            return Ok(groupedResult);
        }
        
        [HttpGet("GetTaskDetails/{TaskId:int}")]
        public async Task<IActionResult> GetTaskDetails(int TaskId)
        {
            var query = from t in dbContext.TasksForUsers
                        join n in dbContext.Notes on t.Id equals n.TaskId
                        join e in dbContext.Employees on t.EmployeeId equals e.Id
                        where t.Id == TaskId
                        select new
                        {
                            Task = t,
                            Note = n,
                            Employee = e
                        };

            var result = await query.ToListAsync();

            var groupedResult = result
                .GroupBy(x => new { x.Task, x.Employee })
                .Select(g => new
                {
                    Task = g.Key.Task,
                    Employee = g.Key.Employee,
                    Notes = g.Select(x => x.Note).ToList()
                })
                .FirstOrDefault();

            return Ok(groupedResult);
        }

        [HttpPut("MarkTaskCompleted/{id:int}")]
        public async Task<IActionResult> MarkTaskCompleted(int id)
        {
            var task = await dbContext.TasksForUsers.FindAsync(id);
            if (task == null)
                return NotFound(JsonSerializer.Serialize("No Task found"));
            
            task.Title = task.Title;
            task.Description = task.Description;
            task.DueDate = task.DueDate;
            task.IsCompleted = true;
            task.EmployeeId = task.EmployeeId;

            await dbContext.SaveChangesAsync();

            return Ok(task);
        }

        [HttpGet("GetNoteForTask/{taskId:int}")]
        public async Task<IActionResult> GetNoteForTask(int taskId)
        {
            var query = from n in dbContext.Notes
                        join t in dbContext.TasksForUsers on n.TaskId equals t.Id
                        where t.Id == taskId
                        select new
                        {
                            Note = n,
                            Task = t
                        };

            var result = await query.ToListAsync();

            var groupedResult = result
                .GroupBy(x => x.Task)
                .Select(g => new
                {
                    Task = g.Key,
                    Notes = g.Select(x => x.Note).ToList()
                })
                .FirstOrDefault();

            return Ok(groupedResult);

        }

        [HttpGet("GetEmpForManager/{id:int}")]
        public async Task<IActionResult> GetEmpForManager(int id)
        {
            var query = from e in dbContext.Employees
                        join t in dbContext.TasksForUsers on e.Id equals t.EmployeeId
                        where e.ManagerId == id
                        orderby e.Id, t.Id
                        group new { e, t } by new { e.Id, e.Name, e.Position } into g
                        select new
                        {
                            EmployeeId = g.Key.Id,
                            EmployeeName = g.Key.Name,
                            EmployeePosition = g.Key.Position,
                            Tasks = g.Select(x => new
                            {
                                TaskId = x.t.Id,
                                TaskTitle = x.t.Title,
                                TaskDescription = x.t.Description,
                                TaskDueDate = x.t.DueDate,
                                TaskIsCompleted = x.t.IsCompleted
                            }).ToList()
                        };

            var result = await query.ToListAsync();

            return Ok(result);
        }

        [HttpGet("GetAdminReport")]
        public async Task<IActionResult> GetAdminReport()
        {
            var query = from e in dbContext.Employees
                        join t in dbContext.TasksForUsers on e.Id equals t.EmployeeId
                        join m in dbContext.Employees on e.ManagerId equals m.Id into managerGroup
                        from manager in managerGroup.DefaultIfEmpty()
                        where e.Position != "admin"
                        select new
                        {
                            EmployeeName = e.Name,
                            ManagerName = manager != null ? manager.Name : null,
                            TaskTitle = t.Title,
                            TaskDescription = t.Description,
                            TaskDueDate = t.DueDate,
                            TaskIsCompleted = t.IsCompleted
                        };

            var result = await query.ToListAsync();

            return Ok(result);

        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] int taskId, [FromForm] string content)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided");  

            var uploadDir = Path.Combine(_env.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var filePath = Path.Combine(uploadDir, Guid.NewGuid() + Path.GetExtension(file.FileName));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var note = new Note
            {
                Content = content,
                TaskId = taskId,
                PathToDoc = filePath
            };

            dbContext.Notes.Add(note);
            await dbContext.SaveChangesAsync();

            return Ok(new { filePath });
        }

    }
}
