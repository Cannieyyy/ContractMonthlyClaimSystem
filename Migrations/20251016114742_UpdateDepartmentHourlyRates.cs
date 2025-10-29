using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractMonthlyClaimSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDepartmentHourlyRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentID",
                keyValue: 1,
                column: "Name",
                value: "Diploma in Software Development");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentID",
                keyValue: 2,
                column: "Name",
                value: "Bachelor in Information Technology");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentID",
                keyValue: 3,
                column: "Name",
                value: "Higher Certificate In Networking");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentID",
                keyValue: 4,
                column: "Name",
                value: "Diploma in Web Development");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentID",
                keyValue: 1,
                column: "Name",
                value: "Human Resources");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentID",
                keyValue: 2,
                column: "Name",
                value: "Computer Science");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentID",
                keyValue: 3,
                column: "Name",
                value: "Mathematics");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentID",
                keyValue: 4,
                column: "Name",
                value: "Finance");
        }
    }
}
