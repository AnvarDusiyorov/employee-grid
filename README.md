<!-- TABLE OF CONTENTS -->
<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ol>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ol>
    </li>
    <li><a href="#usage">Usage</a></li>
  </ol>
</details>


<!-- ABOUT THE PROJECT -->
## About The Project
  Web application for parsing CSV files via uploading CSV file to the app to display the content of the uploaded CSV file in sortable, editable and filterable grid.
  The web application: 
  - supports only CSV files with headers like in [this CSV file][rolemodel-csv-file] 
  - saves employees from an uploaded CSV file into database

### Built With
  * [ASP.NET Core](https://github.com/dotnet/aspnetcore)   
  * [GridMvcCore](https://github.com/gustavnavar/Grid.Blazor/blob/master/docs/dotnetcore/Documentation.md)
  * [Entity Framework Core](https://github.com/dotnet/efcore) 
  * [CsvHelper](https://github.com/JoshClose/CsvHelper)
  * [htmx](https://github.com/bigskysoftware/htmx)

## Getting Started
To see the project in action do the following steps:

### Prerequisites
* Download and install [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Installation
1. Create database ```employee_grid_db``` in SQL Server
2. Use dotnet CLI command ```dotnet ef update database```

## Usage
  * After successful installation and running the app you will see:
  ![welcome-image]

  * If you forget to provide CSV file, you will see:
  ![no-file-provided-image]

  * If you provide valid CSV file (by valid I mean any CSV file with header like in [this][rolemodel-csv-file]), you will see:
  ![valid-file-provided-image]
  
  * After pressing "Upload" button, you will see something like this:
  ![successfully-added-rows-image]
  
  * You can view all employees that are successfully added to the database:
  ![show-all-image]

  * You can use common searching:
   ![search-michael-image]
  
  * You can use filters:
   ![using-filters-image]

  * You can use inline editing:
   ![inline-editing-textfield-image]
  * But note that you can't save an empty text:
    ![forcing-provide-not-empty-text]
  * And you can't save some string without ```@``` symbol in it:
    ![forcing-provide-not-empty-email]

[rolemodel-csv-file]: https://github.com/AnvarDusiyorov/employee-grid/blob/master/EmployeeGridTests/CsvFiles/rolemodel.csv
[welcome-image]: https://github.com/AnvarDusiyorov/employee-grid/blob/master/Screenshots/1.png
[no-file-provided-image]: https://github.com/AnvarDusiyorov/employee-grid/blob/master/Screenshots/2%20No%20file%20provided.png
[valid-file-provided-image]: https://github.com/AnvarDusiyorov/employee-grid/blob/master/Screenshots/3%20Valid%20file%20provided.png
[successfully-added-rows-image]: https://github.com/AnvarDusiyorov/employee-grid/blob/master/Screenshots/4%20Successfully%20added%20all%203%20rows.png
[show-all-image]: https://github.com/AnvarDusiyorov/employee-grid/blob/master/Screenshots/5%20Showing%20all%203%20items.png
[search-michael-image]: https://github.com/AnvarDusiyorov/employee-grid/blob/master/Screenshots/6%20Searching%20employee%20Michael.png
[using-filters-image]: https://github.com/AnvarDusiyorov/employee-grid/blob/master/Screenshots/7%20Using%20filters.png
[inline-editing-textfield-image]: https://github.com/AnvarDusiyorov/employee-grid/blob/master/Screenshots/8%20Inline%20editing%20text%20field.png
[forcing-provide-not-empty-text]: https://github.com/AnvarDusiyorov/employee-grid/blob/master/Screenshots/9%20Forcing%20to%20provide%20not%20empty%20field.png
[inline-editing-emailfield]: https://github.com/AnvarDusiyorov/employee-grid/blob/master/Screenshots/10%20Inline%20editing%20email%20field.png
[forcing-provide-not-empty-email]: https://github.com/AnvarDusiyorov/employee-grid/blob/master/Screenshots/11%20Forcing%20to%20provide%20email%20field.png
