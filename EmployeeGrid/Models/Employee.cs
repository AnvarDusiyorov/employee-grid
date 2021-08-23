using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeGrid.Models
{
    public class Employee
    {
        public long Id { get; set; }
        public string PayrollNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public string Telephone { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string SecondAdress { get; set; }
        public string Postcode { get; set; }
        public string EmailHome { get; set; }
        public DateTime StartDate { get; set; }
    }
}
