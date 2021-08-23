using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmployeeGrid.Infrastructure
{
    public class FileUploadingConfig
    {
        public long FileSizeLimit { get; set; } 
        public string StoredFilesPath { get; set; }
        
        public long FileSizeLimitInMegabytes { get { return FileSizeLimit / (1024 * 1024); } }
    }
}
