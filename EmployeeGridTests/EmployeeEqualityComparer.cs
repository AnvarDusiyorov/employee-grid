using EmployeeGrid.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EmployeeGridTests
{
    public class EmployeeEqualityComparer : IEqualityComparer<Employee>
    {
        public bool Equals(Employee x, Employee y)
        {
            if (x == null && y == null)
                return true;
            else if (x == null || y == null)
                return false;

            if (x.PayrollNumber == y.PayrollNumber && x.FirstName == y.FirstName && x.LastName == y.LastName && 
               x.Birthday == y.Birthday && x.Telephone == y.Telephone && x.Mobile == y.Mobile &&
               x.Address == y.Address && x.SecondAdress == y.SecondAdress && x.Postcode == y.Postcode &&
               x.EmailHome == y.EmailHome && x.StartDate == y.StartDate
               ) return true;
            
            return false;
        }

        public int GetHashCode([DisallowNull] Employee obj)
        {
            var temp = $"{obj.PayrollNumber}-{obj.FirstName}-{obj.LastName}-{obj.Birthday.ToString()}-{obj.Telephone}"
                + $"-{obj.Mobile}-{obj.Address}-{obj.SecondAdress}-{obj.Postcode}-{obj.EmailHome}-{obj.StartDate.ToString()}";
            
            return temp.GetHashCode();
        }
    }
}