using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractMonthlyClaimSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentID",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportingDocuments_Claims_ClaimID",
                table: "SupportingDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_Employees_EmployeeID",
                table: "UserAccounts");

            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_EmployeeID",
                table: "UserAccounts");

            migrationBuilder.CreateTable(
                name: "Verification",
                columns: table => new
                {
                    VerificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimID = table.Column<int>(type: "int", nullable: false),
                    VerifiedBy = table.Column<int>(type: "int", nullable: false),
                    VerificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verification", x => x.VerificationID);
                    table.ForeignKey(
                        name: "FK_Verification_Claims_ClaimID",
                        column: x => x.ClaimID,
                        principalTable: "Claims",
                        principalColumn: "ClaimID");
                    table.ForeignKey(
                        name: "FK_Verification_Employees_VerifiedBy",
                        column: x => x.VerifiedBy,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_EmployeeID",
                table: "UserAccounts",
                column: "EmployeeID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_EmployeeID",
                table: "Claims",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Verification_ClaimID",
                table: "Verification",
                column: "ClaimID");

            migrationBuilder.CreateIndex(
                name: "IX_Verification_VerifiedBy",
                table: "Verification",
                column: "VerifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Employees_EmployeeID",
                table: "Claims",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentID",
                table: "Employees",
                column: "DepartmentID",
                principalTable: "Departments",
                principalColumn: "DepartmentID");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportingDocuments_Claims_ClaimID",
                table: "SupportingDocuments",
                column: "ClaimID",
                principalTable: "Claims",
                principalColumn: "ClaimID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccounts_Employees_EmployeeID",
                table: "UserAccounts",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Employees_EmployeeID",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentID",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportingDocuments_Claims_ClaimID",
                table: "SupportingDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_Employees_EmployeeID",
                table: "UserAccounts");

            migrationBuilder.DropTable(
                name: "Verification");

            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_EmployeeID",
                table: "UserAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Claims_EmployeeID",
                table: "Claims");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_EmployeeID",
                table: "UserAccounts",
                column: "EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentID",
                table: "Employees",
                column: "DepartmentID",
                principalTable: "Departments",
                principalColumn: "DepartmentID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportingDocuments_Claims_ClaimID",
                table: "SupportingDocuments",
                column: "ClaimID",
                principalTable: "Claims",
                principalColumn: "ClaimID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccounts_Employees_EmployeeID",
                table: "UserAccounts",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
