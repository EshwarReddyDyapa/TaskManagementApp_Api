using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManagementApp.Models;
using static Azure.Core.HttpHeader;

namespace TaskManagementApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskManagementController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public TaskManagementController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // To-Do BEFORE UI
        // 1. Show all employees - Done
        // get method from EmployeeController

        // 2. Show Tasks for the selected user - Done
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
        // 3. On selecting a task, display all the details of task along with Note
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


        // To-Do AFTER UI
        // 1. Edit functionality for tasks - Not required
        // 2. Add new Task for a user - Done
        // 3. Mark a task as Completed - Done

        // 4. Add Notes for a task - Done
        // 5. Delete Note - Done

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

        // To-Do FINAL
        // 1. Show Manager Dashboard
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
        // 2. Admin report
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

    }
}
