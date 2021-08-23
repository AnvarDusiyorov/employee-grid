using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeGrid.Validation
{
    /// <summary>
    /// Attribute for IFormFile to check whether extension of IFormFile is permitted 
    /// </summary>
    public class FormFileExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _permittedExtensions;
        public FormFileExtensionsAttribute(params string[] permittedExtensions)
        {
            _permittedExtensions = permittedExtensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var formFile = value as IFormFile;

            if (formFile == null)
                return new ValidationResult("No file is provided");

            var extension = Path.GetExtension(formFile.FileName).ToLowerInvariant();
            if (_permittedExtensions.Contains(extension))
                return ValidationResult.Success;

            return new ValidationResult("Provided file extension is not permitted!");
        }
    }
}
