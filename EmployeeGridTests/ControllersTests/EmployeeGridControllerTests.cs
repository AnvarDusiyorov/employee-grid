using EmployeeGrid.Controllers;
using EmployeeGrid.Infrastructure;
using EmployeeGrid.Models;
using EmployeeGrid.Services;
using EmployeeGridTests.ServicesTests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeGridTests.ControllersTests
{
    class EmployeeGridControllerTests
    {
        private const string INDEX_VIEW_NAME = "Index";

        [Test]
        public async Task UploadSingleFileReturns_IndexViewResult_WhenModelIsNotValid()
        {
            //Arrange
            var fileUploadConfig = new FileUploadingConfig { FileSizeLimit = 2097152, StoredFilesPath = "c:\\files" };
            var controller = new EmployeeGridController(fileUploadConfig, null, null, null);
            controller.ModelState.AddModelError("FileUpload.FormFile", "No file is provided"); 
            // That could be any model error as we need just some not valid model state 

            // Act 
            var result = await controller.UploadSingleFile();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.AreEqual(INDEX_VIEW_NAME, viewResult.ViewName);
        }

        [Test]
        public async Task UploadSingleFileReturns_IndexViewResult_And_AddsModelStateError_WhenUploadedFileSizeIsTooLarge()
        {
            //Arrange
            var expectedKey = GetExpectedModelStateKey();

            var fileUploadConfig = new FileUploadingConfig { FileSizeLimit = 1024 };
            var controller = new EmployeeGridController(fileUploadConfig, null, null, null); 
            
            var fileMock = GetFormFileMock(expectedKey, fileUploadConfig.FileSizeLimit + 100); // Creating formFile mock

            var fileUploadModel = new BufferedSingleFileUploadModel { FormFile = fileMock.Object };
            controller.FileUpload = fileUploadModel;

            // Act 
            var result = await controller.UploadSingleFile();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.AreEqual(INDEX_VIEW_NAME, viewResult.ViewName);
            Assert.That(controller.ModelState.Keys, Contains.Item(expectedKey));
        }

        [Test]
        public async Task UploadSingleFileReturns_RedirectionToIndex_WhenModelIsValid()
        {
            // Arrange
            var controller = GetControllerWithMocks();
            
            // Act
            var result = await controller.UploadSingleFile();

            // Assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.IsNotNull(controller.TempData[nameof(controller.SuccessfullyAddedRecords)]);
            Assert.IsNotNull(controller.TempData[nameof(controller.Employees)]);
        }

        [Test]
        public async Task UploadSingleFile_ReturnsIndexViewResult_WhenCsvFileHeadersAreIncorrect()
        {
            // Arrange
            var employeeCsvHelperServiceMock = new Mock<IEmployeeCsvHelperService>();
            employeeCsvHelperServiceMock.Setup(_ => _.ReadEmployeesFromCsvFile(It.IsAny<IFormFile>()))
                .Throws(new Exception()); // throws exception like the service got csv file with incorrect headers  

            var fileUploadConfig = new FileUploadingConfig { FileSizeLimit = 4024 };
            var controller = new EmployeeGridController(fileUploadingConfig: fileUploadConfig,
                    employeeCsvHelperService : employeeCsvHelperServiceMock.Object,
                    employeeService: null,
                    logger: null
                );

            var expectedKey = GetExpectedModelStateKey();
            var fileMock = GetFormFileMock(expectedKey, fileUploadConfig.FileSizeLimit - 10);
            
            controller.FileUpload = new BufferedSingleFileUploadModel { FormFile = fileMock.Object };
            
            // Act
            var result = await controller.UploadSingleFile();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.AreEqual(INDEX_VIEW_NAME, viewResult.ViewName);
            Assert.That(controller.ModelState.Keys, Contains.Item(expectedKey));
        }

        private Mock<IFormFile> GetFormFileMock(string expectedKey, long mockFileSize)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.SetupGet(_ => _.Length).Returns(mockFileSize);
            fileMock.SetupGet(_ => _.Name).Returns(expectedKey);
            return fileMock;
        }

        private string GetExpectedModelStateKey()
        {
            var propertyInfoThatIsFileUploadModel = typeof(EmployeeGridController).GetProperties()
                .Single(x => x.PropertyType == typeof(BufferedSingleFileUploadModel));

            var infoOfPropertyThatIsIFormFile = typeof(BufferedSingleFileUploadModel).GetProperties()
                .Single(x => x.PropertyType == typeof(IFormFile));

            return $"{propertyInfoThatIsFileUploadModel.Name}.{infoOfPropertyThatIsIFormFile.Name}";
        }

        private EmployeeGridController GetControllerWithMocks()
        {
            var mockList = new List<Employee> { new Employee { FirstName = "Ted" } };
            var employeeCsvHelperServiceMock = new Mock<IEmployeeCsvHelperService>();
            employeeCsvHelperServiceMock.Setup(_ => _.ReadEmployeesFromCsvFile(It.IsAny<IFormFile>()))
                .Returns(mockList);
            // Creating mock of employeeCsvHelperService

            var employeeServiceMock = new Mock<IEmployeeService>();
            employeeServiceMock
                .Setup(_ => _.CreateEmployees(It.IsAny<List<Employee>>()))
                .Returns(Task.FromResult(mockList));
            // Creating mock of employeeService

            var fileUploadConfig = new FileUploadingConfig { FileSizeLimit = 1024 };
            var controller = new EmployeeGridController(fileUploadingConfig: fileUploadConfig,
                employeeService: employeeServiceMock.Object,
                employeeCsvHelperService: employeeCsvHelperServiceMock.Object,
                logger: null);

            // Creating file mock
            var fileMock = new Mock<IFormFile>();
            var fileUploadModel = new BufferedSingleFileUploadModel { FormFile = fileMock.Object };
            controller.FileUpload = fileUploadModel;

            // Creating TempData with mock data provider
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            return controller;
        }
    }
}
