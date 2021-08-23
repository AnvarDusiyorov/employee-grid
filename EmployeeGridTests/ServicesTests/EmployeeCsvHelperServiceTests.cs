using EmployeeGrid.Services;
using EmployeeGrid.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeGridTests.ServicesTests
{
    class EmployeeCsvHelperServiceTests
    {
        public const string VALID_CSV_FILE_NAME = "validDataset.csv"; // that csv file contains valid data
        public const string ROLE_MODEL_CSV_FILE_NAME = "rolemodel.csv"; // that csv file defines acceptable headers
        public const string INCORRECT_CSV_FILE_NAME = "incorrect.csv"; // that csv file contains incorrect headers

        [Test]
        public void AreAttributesOnEmployeeDetailsCorrect()
        {
            // Test to check whether name attributes on EmployeeDetail properties are correct

            var dict = Helper.GetNameAttributePropertiesDictionary(typeof(EmployeeDetail));

            var roleModelCsvFilePath = Helper.GetCsvFilePathByFilename(ROLE_MODEL_CSV_FILE_NAME);

            var headers = Helper.GetHeadersOfCsvFile(roleModelCsvFilePath);

            foreach (var x in dict)
            {
                if (!headers.Contains(x.Key))
                {
                    Assert.Fail($"Name attribute on {x.Value} property of {nameof(EmployeeDetail)} is incorrect");
                }
            }
            //foreach(var header in headers)
            //{
            //    Assert.That(dict.Keys, Contains.Item(header));
            //}
        }

        [Test]
        public void CanParseValidCsvFile()
        {
            // Arrange
            var expectedListOfEmployees = GetExpectedEmployeesForValidCsvFile();

            var filePathOfValidCsvFile = Helper.GetCsvFilePathByFilename(VALID_CSV_FILE_NAME);
            var expectedFormFile = Helper.GetFormFile(filePathOfValidCsvFile);

            var employeeCsvHelperService = new EmployeeCsvHelperService();

            // Act 
            var actualListOfEmployees = employeeCsvHelperService.ReadEmployeesFromCsvFile(expectedFormFile);

            var difference = expectedListOfEmployees.Except(actualListOfEmployees, new EmployeeEqualityComparer()).ToList();
            var expectedDifference = new List<Employee>();

            // Assert
            Assert.AreEqual(expectedDifference, difference);
        }

        [Test]
        public void ThrowsSpecificException_WhenCsvFileHeadersAreIncorrect()
        {
            // Arrange
            var filePathOfIncorrectCsvFile = Helper.GetCsvFilePathByFilename(INCORRECT_CSV_FILE_NAME);
            var expectedFormFile = Helper.GetFormFile(filePathOfIncorrectCsvFile);
            var employeeCsvHelperService = new EmployeeCsvHelperService();

            // Act and assert
            Assert.Throws<CsvHelper.HeaderValidationException>(() => employeeCsvHelperService.ReadEmployeesFromCsvFile(expectedFormFile));
        }

        private List<Employee> GetExpectedEmployeesForValidCsvFile()
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
    }
}
