using EmployeeGrid.Data;
using EmployeeGrid.Models;
using EmployeeGrid.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeGridTests.ServicesTests
{
    class EmployeeServiceTests
    {
        [Test]
        public async Task CreateEmployees_DoesntAddEmployees_WithDuplicatePayrollNumber()
        {
            // Arrange 
            var (connection, options) = await GetConnectionAndOptions();
            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var service = new EmployeeService(context);

            var duplicateEmployees = GetEmployeesWithDuplicatePayrollNumber();

            // Act 
            var actualEmployees = await service.CreateEmployees(duplicateEmployees);

            // Assert
            var expectedCount = duplicateEmployees.Select(x => x.PayrollNumber).Distinct().Count();
            Assert.AreEqual(expectedCount, actualEmployees.Count);
            Assert.Greater(actualEmployees.Count, 0);
        }

        [Test]
        public async Task EditEmployee_ThrowsException_IfEmployeeWithProvidedIdDoesntExist()
        {
            // Arrange 
            var (connection, options) = await GetConnectionAndOptions();
            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var service = new EmployeeService(context);
            var validEmployees = GetValidEmployees();
            await service.CreateEmployees(validEmployees);

            var maxId = context.Employees.Max(x => x.Id);
            var employee = await context.Employees.FindAsync(maxId);
            var nonExistingId = maxId + 1;
            var propertyName = nameof(employee.PayrollNumber);
            var newValue = validEmployees[0].PayrollNumber + validEmployees[0].PayrollNumber;

            // Act and Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => service.EditEmployee(nonExistingId, propertyName, newValue));
        }

        [Test]
        public async Task EditEmployee_ThrowsException_IfPropertyOfEmployeeClassDoesntExist()
        {
            // Arrange 
            var (connection, options) = await GetConnectionAndOptions();
            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var service = new EmployeeService(context);
            var validEmployees = GetValidEmployees();
            await service.CreateEmployees(validEmployees);

            var maxId = context.Employees.Max(x => x.Id);
            var employee = await context.Employees.FindAsync(maxId);
            var propertyName = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var newValue = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

            // Act and assert
            Assert.ThrowsAsync<ArgumentException>(() => service.EditEmployee(maxId, propertyName, newValue));
        }

        [Test]
        public async Task EditEmployee_ThrowsException_IfProvidedPropertyTypeDoesntCorrespond()
        {
            // Arrange 
            var (connection, options) = await GetConnectionAndOptions();
            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var service = new EmployeeService(context);
            var validEmployees = GetValidEmployees();
            await service.CreateEmployees(validEmployees);

            var maxId = context.Employees.Max(x => x.Id);
            var employee = await context.Employees.FindAsync(maxId);
            var propertyName = nameof(employee.PayrollNumber);
            var newValue = DateTime.Now;

            // Act and assert
            Assert.ThrowsAsync<ArgumentException>(() => service.EditEmployee(maxId, propertyName, newValue));
        }

        [Test]
        public async Task EditEmployee_ThrowsDbUpdateException_IfPayrollNumberAlreadyExist()
        {
            // Arrange 
            var (connection, options) = await GetConnectionAndOptions();
            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var service = new EmployeeService(context);
            var validEmployees = GetValidEmployees();
            await service.CreateEmployees(validEmployees);

            var employee = await context.Employees.SingleAsync(x => x.PayrollNumber == validEmployees[0].PayrollNumber);
            var propertyName = nameof(employee.PayrollNumber);
            var id = employee.Id;
            var newValue = validEmployees[1].PayrollNumber;

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateException>(() => service.EditEmployee(id, propertyName, newValue));
        }

        [Test]
        public async Task EditEmployee_SavesSuccessfully_IfAllRight()
        {
            // Arrange 
            var (connection, options) = await GetConnectionAndOptions();
            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var service = new EmployeeService(context);
            var validEmployees = GetValidEmployees();
            await service.CreateEmployees(validEmployees);

            var employee = await context.Employees.SingleAsync(x => x.PayrollNumber == validEmployees[0].PayrollNumber);
            var propertyName = nameof(employee.PayrollNumber);
            var id = employee.Id;
            var newValue = validEmployees[0].PayrollNumber + validEmployees[1].PayrollNumber;

            // Act and assert
            Assert.DoesNotThrowAsync(() => service.EditEmployee(id, propertyName, newValue));
            Assert.AreEqual(newValue, context.Employees.Find(id).PayrollNumber);
        }

        [Test]
        public async Task EditEmployee_SavesSuccessfully_AfterTryingToUpdateWithDuplicatePayrollNumber()
        {
            // Arrange 
            var (connection, options) = await GetConnectionAndOptions();
            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var service = new EmployeeService(context);
            var validEmployees = GetValidEmployees();
            await service.CreateEmployees(validEmployees);

            var employee = await context.Employees.SingleAsync(x => x.PayrollNumber == validEmployees[0].PayrollNumber);
            var propertyName = nameof(employee.PayrollNumber);
            var id = employee.Id;
            var newValueForFail = validEmployees[1].PayrollNumber;
            var newValueForSuccess = validEmployees[0].PayrollNumber + validEmployees[1].PayrollNumber;

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateException>(() => service.EditEmployee(id, propertyName, newValueForFail));
            Assert.DoesNotThrowAsync(() => service.EditEmployee(id, propertyName, newValueForSuccess));
        }

        [Test]
        public async Task EditEmployee_DoesntDelete_After_TryingToUpdateWithDuplicatePayrollNumber_SavingChanges()
        {
            // Arrange 
            var (connection, options) = await GetConnectionAndOptions();
            using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var service = new EmployeeService(context);
            var validEmployees = GetValidEmployees();
            await service.CreateEmployees(validEmployees);

            var employee = await context.Employees.SingleAsync(x => x.PayrollNumber == validEmployees[0].PayrollNumber);
            var propertyName = nameof(employee.PayrollNumber);
            var id = employee.Id;
            var newValueForFail = validEmployees[1].PayrollNumber;
            var newValueForSuccess = validEmployees[0].PayrollNumber + validEmployees[1].PayrollNumber;

            // Act and Assert
            Assert.ThrowsAsync<DbUpdateException>(() => service.EditEmployee(id, propertyName, newValueForFail));
            Assert.DoesNotThrowAsync(() => context.SaveChangesAsync());
            Assert.DoesNotThrowAsync(() => context.Employees.SingleAsync(x => x.Id == id));
            Assert.DoesNotThrowAsync(() => service.EditEmployee(id, propertyName, newValueForSuccess));
        }

        public List<Employee> GetValidEmployees()
        {
            return new List<Employee>
            {
                new Employee
                {
                    PayrollNumber = "COOP08", FirstName = "John", LastName = "William",
                    Birthday = new DateTime(1955, 1, 26), Telephone = "12345678", Mobile = "987654231",
                    Address = "12 Foreman road", SecondAdress = "London", Postcode = "GU12 6JW",
                    EmailHome = "nomadic20@hotmail.co.uk", StartDate = new DateTime(2013, 4, 18)
                },
                new Employee
                {
                    PayrollNumber = "JACK13", FirstName = "Jerry", LastName = "Jackson",
                    Birthday = new DateTime(1974, 5, 11), Telephone = "2050508", Mobile = "6987457",
                    Address = "115 Spinney Road", SecondAdress = "Luton", Postcode = "LU33DF",
                    EmailHome = "gerry.jackson@bt.com", StartDate = new DateTime(2013, 4, 18)
                }
            };
        }

        private List<Employee> GetEmployeesWithDuplicatePayrollNumber()
        {
            var payrollNumber = "COOP08";
            return new List<Employee>
            {
                new Employee
                {
                    PayrollNumber = payrollNumber, FirstName = "John", LastName = "William",
                    Birthday = new DateTime(1955, 1, 26), Telephone = "12345678", Mobile = "987654231",
                    Address = "12 Foreman road", SecondAdress = "London", Postcode = "GU12 6JW",
                    EmailHome = "nomadic20@hotmail.co.uk", StartDate = new DateTime(2013, 4, 18)
                },
                new Employee
                {
                    PayrollNumber = payrollNumber, FirstName = "Jerry", LastName = "Jackson",
                    Birthday = new DateTime(1974, 5, 11), Telephone = "2050508", Mobile = "6987457",
                    Address = "115 Spinney Road", SecondAdress = "Luton", Postcode = "LU33DF",
                    EmailHome = "gerry.jackson@bt.com", StartDate = new DateTime(2013, 4, 18)
                },
                new Employee
                {
                    PayrollNumber = payrollNumber + "S", FirstName = "Jerry", LastName = "Jackson",
                    Birthday = new DateTime(1974, 5, 11), Telephone = "2050508", Mobile = "6987457",
                    Address = "115 Spinney Road", SecondAdress = "Luton", Postcode = "LU33DF",
                    EmailHome = "gerry.jackson@bt.com", StartDate = new DateTime(2013, 4, 18)
                }
            };
        }

        private async Task<(SqliteConnection, DbContextOptions<AppDbContext>)> GetConnectionAndOptions()
        {
            var connection = new SqliteConnection("Datasource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            return (connection, options);
        }
    }

}
