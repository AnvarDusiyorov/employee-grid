using EmployeeGrid.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeGrid.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) :
            base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                        .HasIndex(x => x.PayrollNumber)
                        .IsUnique(); // Adding uniqueness constraint on payroll number of an employee
        }

        public DbSet<Employee> Employees { get; set; }
    }
}
