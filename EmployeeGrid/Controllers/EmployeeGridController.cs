using EmployeeGrid.Infrastructure;
using EmployeeGrid.Models;
using EmployeeGrid.Services;
using EmployeeGrid.Validation;
using GridMvc.Server;
using GridShared;
using GridShared.Sorting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeGrid.Controllers
{
    public class EmployeeGridController : Controller
    {
        public const string ACTION_NAME_TO_GET_FORM = nameof(GetFormForEditingProperty);
        public readonly string MANDATORY_HTML_TIME_FORMAT = "yyyy-MM-dd"; // that format is mandatory for <input type="date"> to work correctly
        public readonly InputType DATE = InputType.date;
        public readonly InputType EMAIL = InputType.email;
        public readonly InputType NUMBER = InputType.number;

        [BindProperty]
        public string NewStringValue { get; set; }
        [BindProperty]
        public DateTime NewDateTimeValue { get; set; }

        private readonly ILogger<EmployeeGridController> _logger;
        private readonly FileUploadingConfig fileUploadingConfig;
        [BindProperty]
        public BufferedSingleFileUploadModel FileUpload { get; set; }
        

        public List<Employee> Employees { get; set; }

        [ViewData]
        public long FileSizeLimitInMegabytes { get; init; }

        public int SuccessfullyAddedRecords { get; }

        private readonly IEmployeeService employeeService;
        private readonly IEmployeeCsvHelperService employeeCsvHelperService;

        public EmployeeGridController(
            FileUploadingConfig fileUploadingConfig,
            IEmployeeService employeeService,
            IEmployeeCsvHelperService employeeCsvHelperService, ILogger<EmployeeGridController> logger)
        {
            this.fileUploadingConfig = fileUploadingConfig;
            this.FileSizeLimitInMegabytes = fileUploadingConfig.FileSizeLimitInMegabytes;
            this.employeeService = employeeService;
            this.employeeCsvHelperService = employeeCsvHelperService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            if(TempData[nameof(SuccessfullyAddedRecords)] != null)
            {
                // getting number of successfully added employees to show the number to user
                ViewData[nameof(SuccessfullyAddedRecords)] = (int)TempData[nameof(SuccessfullyAddedRecords)];
                TempData[nameof(SuccessfullyAddedRecords)] = null;
            }

            if(TempData[nameof(Employees)] != null)  
            {
                // Getting Employees data from TempData
                Employees = JsonConvert.DeserializeObject<List<Employee>>(TempData[nameof(Employees)] as string);
            }

            TempData[nameof(Employees)] = JsonConvert.SerializeObject(Employees); // saving Employees data to TempData

            return View("Index", this);
        }

        [HttpPost]
        public async Task<IActionResult> UploadSingleFile()
        {
            if (!ModelState.IsValid) return View("Index", this);

            if (FileUpload.FormFile.Length > fileUploadingConfig.FileSizeLimit) // checking size of uploaded file
            {
                ModelState.AddModelError(FileUpload.FormFile.Name, $"The file size exceeds {FileSizeLimitInMegabytes} megabytes!");
                return View("Index", this);
            }

            try
            {
                Employees = employeeCsvHelperService.ReadEmployeesFromCsvFile(FileUpload.FormFile);
            }catch(Exception)
            {
                ModelState.AddModelError(FileUpload.FormFile.Name, "Headers of provided csv file is incorrect!");
                return View("Index", this);
            }

            Employees = await employeeService.CreateEmployees(Employees);

            TempData[nameof(SuccessfullyAddedRecords)] = Employees.Count; // saving number of successfully added employees 
            TempData[nameof(Employees)] = JsonConvert.SerializeObject(Employees); // saving Employees data to TempData

            return RedirectToAction("Index");
        }

        public string GetDefaultCellView(long id, string propertyName, string recordValue, InputType inputType = InputType.text, string actionName = ACTION_NAME_TO_GET_FORM)
        {
            var controllerName = this.ControllerContext.ActionDescriptor.ControllerName;
            var url = Url.Action(actionName, controllerName, new
            {
                id = id, 
                propertyName = propertyName,
                recordValue = recordValue,
                inputType = inputType
            });

            return $@"<button class='btn-light' style='min-width:105px' hx-get=""{url}"" hx-swap=""outerHTML"">{recordValue}</button>";
        }

        [HttpGet]
        public string GetFormForEditingProperty(long id, string propertyName, string recordValue, InputType inputType)
        {
            var dict = new Dictionary<InputType,(string, string, string)>
            {
                [InputType.text] = (nameof(InputType.text), nameof(NewStringValue), nameof(EditStringProperty)),
                [InputType.date] = (nameof(InputType.date), nameof(NewDateTimeValue), nameof(EditDateTimeProperty)),
                [InputType.email] = (nameof(InputType.email), nameof(NewStringValue), nameof(EditStringProperty)),
                [InputType.number] = (nameof(InputType.number), nameof(NewStringValue), nameof(EditStringProperty))
            }; // enum -> (real input type, name of controller property to use binding, name of controller action) 

            var (realInputType, propertyNameToBind, actionName) = dict[inputType];            
            var controllerName = this.ControllerContext.ActionDescriptor.ControllerName;
            var parameters = new { id = id, propertyName = propertyName, 
                recordValue = recordValue, inputType = inputType };

            var patchUrl = Url.Action(actionName, controllerName, parameters);
            var getUrl = Url.Action(nameof(GetDefaultCellView), controllerName, parameters);

            return $@"<form hx-patch=""{patchUrl}"" hx-swap=""outerHTML"" hx-target=""this"">
<input type=""{realInputType}"" asp-for=""{propertyNameToBind}"" name=""{propertyNameToBind}"" value=""{recordValue}"" required>
<button class=""btn-primary"">Submit</button>
<button hx-get=""{getUrl}""class=""btn-light"">Cancel</button>
</form>                        ";
        }
        
        [HttpPatch]
        public async Task<string> EditStringProperty(long id, string propertyName, InputType inputType) 
        {
            if(NewStringValue == null)
            {
                _logger.LogWarning($"{nameof(NewStringValue)} binding property is null");
                _logger.LogWarning($"Method = {nameof(EditStringProperty)}, Id = {id}, propertName = {propertyName}");
                return string.Empty;
            }

            try
            {
                var (updatedEmployee, propInfo) = await employeeService.EditEmployee(id, propertyName, NewStringValue);
                UpdateEmployeesListIfPossible(updatedEmployee);
                return GetDefaultCellView(updatedEmployee.Id, propertyName, propInfo.GetValue(updatedEmployee) as string, inputType);
            }
            catch(DbUpdateException e)
            {
                // restoring old string value with default cell view 
                _logger.LogWarning(e.Message);
                var employee = await employeeService.GetEmployee(id);
                var recordValue = employee.GetType().GetProperty(propertyName).GetValue(employee) as string;
                return GetDefaultCellView(id, propertyName, recordValue , inputType);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.Message);
                _logger.LogWarning($"Method = {nameof(EditStringProperty)}, Id = {id}, propertName = {propertyName}, newStringValue = {NewStringValue}");
                return string.Empty;
            }
        }

        [HttpPatch]
        public async Task<string> EditDateTimeProperty(long id, string propertyName, InputType inputType) 
        {
            try
            {
                var (updatedEmployee, propInfo) = await employeeService.EditEmployee(id, propertyName, NewDateTimeValue);
                var newDateTime = (DateTime) propInfo.GetValue(updatedEmployee);
                var recordValue = newDateTime.ToString(MANDATORY_HTML_TIME_FORMAT);
                UpdateEmployeesListIfPossible(updatedEmployee);
                return GetDefaultCellView(updatedEmployee.Id, propertyName, recordValue, inputType);
            }
            catch (DbUpdateException e)
            {
                // restoring old datetime value with default cell view 
                _logger.LogWarning(e.Message);
                var employee = await employeeService.GetEmployee(id);
                var oldDateTime = (DateTime)employee.GetType().GetProperty(propertyName).GetValue(employee);
                var recordValue = oldDateTime.ToString(MANDATORY_HTML_TIME_FORMAT);
                return GetDefaultCellView(id, propertyName, recordValue, inputType);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.Message);
                _logger.LogWarning($"Method = {nameof(EditDateTimeProperty)}, Id = {id}, propertName = {propertyName}, newDateTimeValue = {NewDateTimeValue}");
                return string.Empty;
            }
        }
        
        public void UpdateEmployeesListIfPossible(Employee updatedEmployee)
        {
            if (TempData[nameof(Employees)] != null)
            {
                // Getting Employees data from TempData
                Employees = JsonConvert.DeserializeObject<List<Employee>>(TempData[nameof(Employees)] as string);
                var oldEmployeeIndexInList = Employees.FindIndex(x => x.Id == updatedEmployee.Id);
                Employees[oldEmployeeIndexInList] = updatedEmployee; // update employee in Employees list
                TempData[nameof(Employees)] = JsonConvert.SerializeObject(Employees); // saving Employees data to TempData
            }
        }

        public enum InputType
        {
            text,
            date,
            email,
            number
        }
    }

    public class BufferedSingleFileUploadModel
    {
        [Required(ErrorMessage = "No file is provided!")]
        [FormFileExtensions(".csv")]
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }
    }
}
