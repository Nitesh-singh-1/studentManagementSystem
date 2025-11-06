using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

        // public DbSet<User> Users { get; set; } // uncomment when User model added
    }
}
