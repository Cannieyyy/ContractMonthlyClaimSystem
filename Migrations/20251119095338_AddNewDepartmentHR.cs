using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractMonthlyClaimSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddNewDepartmentHR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "DepartmentID", "HourlyRate", "Name" },
                values: new object[] { 5, 0m, "Human Resources" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "DepartmentID",
                keyValue: 5);
        }
    }
}
