using EmployeeGrid.Data;
using EmployeeGrid.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EmployeeGrid.Services
{
    public interface IEmployeeService
    {
        public Task<List<Employee>> CreateEmployees(List<Employee> employees);
        public Task<(Employee, PropertyInfo)> EditEmployee<T>(long id, string propertyName, T newValue);
        public Task<Employee> GetEmployee(long id);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext appDbContext;

        public EmployeeService(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task<List<Employee>> CreateEmployees(List<Employee> employees)
        {
            var succcessfullyAdded = new List<Employee>(employees.Count);

            foreach (var employee in employees)
            {
                appDbContext.Employees.Add(employee);
                try
                {
                    // if saving changes goes successfully
                    await appDbContext.SaveChangesAsync(); 
                    succcessfullyAdded.Add(employee);
                }
                catch (Exception)
                { 
                    appDbContext.Employees.Remove(employee); 
                }
            }

            return succcessfullyAdded;
        }

        public async Task<(Employee, PropertyInfo)> EditEmployee<T>(long id, string propertyName, T newValue)
        {
            var employee = await appDbContext.Employees.FindAsync(id);
            if (employee == null)
                throw new KeyNotFoundException($"Employee with id={id} doesn't exist!");

            var propInfo = typeof(Employee).GetProperty(propertyName);
            if (propInfo == null)
                throw new ArgumentException($"Employee class doesn't have {propertyName} property!");

            if (propInfo.PropertyType != typeof(T))
                throw new ArgumentException($"Employee class {propertyName} property type is {propInfo.PropertyType}, not {typeof(T)}");

            propInfo.SetValue(employee, newValue);
            appDbContext.Employees.Update(employee);
            try
            {
                await appDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                appDbContext.ChangeTracker.Clear();
                throw new DbUpdateException($"Couldn't update Employee with id={employee.Id}");
            }
                
            return (employee, propInfo);
        }

        public async Task<Employee> GetEmployee(long id)
        {
            var employee = await appDbContext.Employees.FindAsync(id);
            if (employee == null)
                throw new KeyNotFoundException($"Employee with id={id} doesn't exist!");
            return employee;
        }
    }    
}
