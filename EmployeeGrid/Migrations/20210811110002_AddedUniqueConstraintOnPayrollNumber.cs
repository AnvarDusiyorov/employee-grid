using Microsoft.EntityFrameworkCore.Migrations;

namespace EmployeeGrid.Migrations
{
    public partial class AddedUniqueConstraintOnPayrollNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PayrollNumber",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PayrollNumber",
                table: "Employees",
                column: "PayrollNumber",
                unique: true,
                filter: "[PayrollNumber] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_PayrollNumber",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "PayrollNumber",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
