using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManagementApp.Models;

namespace TaskManagementApp.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        { 
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<TasksForUser> TasksForUsers { get; set; }
        public DbSet<Note> Notes { get; set; }

    }
}