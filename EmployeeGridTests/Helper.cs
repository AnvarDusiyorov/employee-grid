using CsvHelper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeGridTests
{
    public static class Helper
    {
        // Name of folder that contains all csv files for testing purposes
        private const string CSV_FILES_FOLDER_NAME = "CsvFiles";

        /// <summary>
        /// Gets full path of root folder of current project
        /// </summary>
        /// <returns>Full path of root folder of current project</returns>
        public static string GetCurrentProjectRootFolderPath()
        {
            var rootFolderPath = Directory.GetCurrentDirectory(); //    ...\EmployeeGridTests\bin\Debug\net5.0
            var parentFolderPath = Directory.GetParent(rootFolderPath).FullName; //     ...\EmployeeGridTests\bin\Debug 

            parentFolderPath = Directory.GetParent(parentFolderPath).FullName; //       ...\EmployeeGridTests\bin
            parentFolderPath = Directory.GetParent(parentFolderPath).FullName; //       ...\EmployeeGridTests

            return parentFolderPath;
        }

        /// <summary>
        /// Gets full path of the csv files folder (i.e. folder that contains all csv files for testing purposes) 
        /// </summary>
        /// <returns>Full path of the csv files folder</returns>
        public static string GetCsvFilesFolderPath()
        {
            var rootFolderPath = GetCurrentProjectRootFolderPath();
            var csvFilesFolderPath = Path.Combine(rootFolderPath, CSV_FILES_FOLDER_NAME);
            return csvFilesFolderPath;
        }

        /// <summary>
        /// Gets full path of csv file via passed csv file name. 
        /// File name must be passed with an extension, e.g. "dataset.csv"
        /// </summary>
        /// <param name="csvFileName"> Csv file name </param>
        /// <returns> Full path of the csv file </returns>
        public static string GetCsvFilePathByFilename(string csvFileName)
        {
            var csvFilesFolderPath = GetCsvFilesFolderPath();
            return Path.Combine(csvFilesFolderPath, csvFileName);
        }

        /// <summary>
        /// Creates FormFile using passed full path of csv file and then returns created FormFile  
        /// </summary>
        /// <param name="csvFileFullPath">Full path of csv file</param>
        /// <returns></returns>
        public static FormFile GetFormFile(string csvFileFullPath)
        {
            var fileStream = new FileStream(csvFileFullPath, FileMode.Open, FileAccess.Read);
            return new FormFile(fileStream, 0, fileStream.Length, null, csvFileFullPath);
        }

        /// <summary>
        /// Creates Dictionary<string, string> where keys are 
        /// name values of CsvHelper.Configuration.Attributes.NameAttribute 
        /// (e.g. if we have attribute [Name("someval")] on some property of some class, then key is "someval")
        /// and values of the dictionary are names of properties of passed classtype.
        /// After creating dictionary returns it
        /// </summary>
        /// <param name="classType">Type of class</param>
        /// <returns> Dictionary where keys are 
        /// name values of CsvHelper.Configuration.Attributes.NameAttribute 
        /// (e.g. if there's attribute [Name("someval")] on some property of some class, then key is "someval")
        /// and values of the dictionary are names of properties of passed classtype. </returns>
        public static Dictionary<string, string> GetNameAttributePropertiesDictionary(System.Type classType)
        {
            var nameAttributeType = typeof(CsvHelper.Configuration.Attributes.NameAttribute);
            var dict = new Dictionary<string, string>();

            foreach (var propertyInfo in classType.GetProperties())
            {
                var nameAttribute = propertyInfo.GetCustomAttribute(nameAttributeType) 
                    as CsvHelper.Configuration.Attributes.NameAttribute;

                if(nameAttribute != null)
                    dict.Add(nameAttribute.Names[0], propertyInfo.Name);
            }

            return dict;
        }

        /// <summary>
        /// Gets headers of csv file using its full path
        /// </summary>
        /// <param name="csvfileFullPath">Full path of csv file</param>
        /// <returns> List of headers of the csv file </returns>
        public static List<string> GetHeadersOfCsvFile(string csvfileFullPath)
        {
            using var sreader = new StreamReader(csvfileFullPath);
            using var csvReader = new CsvReader(sreader, System.Globalization.CultureInfo.InvariantCulture);

            csvReader.Read();
            csvReader.ReadHeader();
            var headers = csvReader.HeaderRecord;
            return headers.ToList();
        }
    }
}
