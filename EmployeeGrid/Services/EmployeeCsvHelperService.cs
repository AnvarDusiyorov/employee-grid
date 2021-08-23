using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using EmployeeGrid.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeGrid.Services
{
    public interface IEmployeeCsvHelperService
    {
        public List<Employee> ReadEmployeesFromCsvFile(IFormFile FormFile);
    }

    public class EmployeeCsvHelperService : IEmployeeCsvHelperService
    {
        public List<Employee> ReadEmployeesFromCsvFile(IFormFile FormFile)
        {
            var employees = new List<Employee>();

            using var sreader = new StreamReader(FormFile.OpenReadStream());
            using var csvReader = new CsvReader(sreader, System.Globalization.CultureInfo.InvariantCulture);

            csvReader.Context.RegisterClassMap<EmployeeDetailMap>();

            try
            {
                var records = csvReader.GetRecords<EmployeeDetail>();
                employees = records.Select(x => x.ToEmployee()).ToList();
            }
            catch (Exception)
            {
                throw;
            }

            return employees;
        }
    }

    /// <summary>
    /// Class for mapping headers in a csv file
    /// </summary>
    public class EmployeeDetailMap : ClassMap<EmployeeDetail>
    {
        public const string PAYROLL_NUMBER_HEADER = "Personnel_Records.Payroll_Number";
        public const string FIRST_NAME_HEADER = "Personnel_Records.Forenames";
        public const string LAST_NAME_HEADER = "Personnel_Records.Surname";
        public const string BIRTHDAY_HEADER = "Personnel_Records.Date_of_Birth";
        public const string TELEPHONE_HEADER = "Personnel_Records.Telephone";
        public const string MOBILE_HEADER = "Personnel_Records.Mobile";
        public const string ADDRESS_HEADER = "Personnel_Records.Address";
        public const string SECOND_ADDRESS_HEADER = "Personnel_Records.Address_2";
        public const string POSTCODE_HEADER = "Personnel_Records.Postcode";
        public const string EMAIL_HOME_HEADER = "Personnel_Records.EMail_Home";
        public const string START_DATE_HEADER = "Personnel_Records.Start_Date";

        public EmployeeDetailMap()
        {
            Map(x => x.PayrollNumber).Name(PAYROLL_NUMBER_HEADER);
            Map(x => x.FirstName).Name(FIRST_NAME_HEADER);
            Map(x => x.LastName).Name(LAST_NAME_HEADER);
            Map(x => x.Birthday).TypeConverterOption.Format("d/M/yyyy").Name(BIRTHDAY_HEADER);
            Map(x => x.Telephone).Name(TELEPHONE_HEADER);
            Map(x => x.Mobile).Name(MOBILE_HEADER);
            Map(x => x.Address).Name(ADDRESS_HEADER);
            Map(x => x.SecondAdress).Name(SECOND_ADDRESS_HEADER);
            Map(x => x.Postcode).Name(POSTCODE_HEADER);
            Map(x => x.EmailHome).Name(EMAIL_HOME_HEADER);
            Map(x => x.StartDate).TypeConverterOption.Format("d/M/yyyy").Name(START_DATE_HEADER);
        }
    }

    public class EmployeeDetail
    {
        [Name(EmployeeDetailMap.PAYROLL_NUMBER_HEADER)]
        public string PayrollNumber { get; set; }

        [Name(EmployeeDetailMap.FIRST_NAME_HEADER)]
        public string FirstName { get; set; }

        [Name(EmployeeDetailMap.LAST_NAME_HEADER)]
        public string LastName { get; set; }

        [Name(EmployeeDetailMap.BIRTHDAY_HEADER)]
        public DateTime Birthday { get; set; }

        [Name(EmployeeDetailMap.TELEPHONE_HEADER)]
        public string Telephone { get; set; }

        [Name(EmployeeDetailMap.MOBILE_HEADER)]
        public string Mobile { get; set; }

        [Name(EmployeeDetailMap.ADDRESS_HEADER)]
        public string Address { get; set; }

        [Name(EmployeeDetailMap.SECOND_ADDRESS_HEADER)]
        public string SecondAdress { get; set; }

        [Name(EmployeeDetailMap.POSTCODE_HEADER)]
        public string Postcode { get; set; }

        [Name(EmployeeDetailMap.EMAIL_HOME_HEADER)]
        public string EmailHome { get; set; }

        [Name("Personnel_Records.Start_Date")]
        public DateTime StartDate { get; set; }

        public Employee ToEmployee()
        {
            return new Employee
            {
                PayrollNumber = this.PayrollNumber.Trim(),
                FirstName = this.FirstName.Trim(),
                LastName = this.LastName.Trim(),
                Birthday = this.Birthday,
                Telephone = this.Telephone.Trim(),
                Mobile = this.Mobile.Trim(),
                Address = this.Address.Trim(),
                SecondAdress = this.SecondAdress.Trim(),
                Postcode = this.Postcode.Trim(),
                EmailHome = this.EmailHome.Trim(),
                StartDate = this.StartDate
            };
        }
    }
}
