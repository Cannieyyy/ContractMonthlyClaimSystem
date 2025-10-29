# Time2Pay — README

**Project:** ContractMonthlyClaimSystem — Time2Pay Portal

**Brief:** ASP.NET Core MVC app that allows lecturers to submit monthly claims (hours worked) with supporting PDF documents. Coordinators/Managers review and approve/reject claims for employees in their department.

## Quick start — run the project

1. Open the solution in Visual Studio (recommended) or run from terminal in project folder containing `ContractMonthlyClaimSystem.csproj`.
2. Build and run the application (F5 in Visual Studio or `dotnet run` from the project folder).
3. On first run the application will **automatically apply migrations and create the database** (this is handled by `Database.Migrate()` in `Program.cs`). You do **not** need to create the database manually.

> **Important:** If you previously changed models, ensure migrations are present in the `Migrations` folder. The app will apply migrations that are committed with the project. If you add model changes locally, run migrations (`dotnet ef migrations add <Name>` and `dotnet ef database update`).

## Registering accounts

1. Open the app and go to the **Login / Register** screen.
2. Fill in **all required fields** on the Register form. Required fields include: Full Name, Email, Department, Role, Password and Confirm Password, and the OTP when required.
3. **Department** is critical. Departments are predefined (seeded) and include their `HourlyRate`. Example departments: *Diploma in Software Development*, *Bachelor in Information Technology*, *Higher Certificate In Networking*, *Diploma in Web Development*.

## Roles & OTP behavior

* **Roles**: `Lecturer`, `Coordinator`, `Manager`.
* **Privileged users** (Coordinator and Manager) require an **OTP** at registration. If the OTP is not provided (or invalid/expired), the registration will be blocked and the user's data will **not** be saved to the database — you will see the block in the console/output prompt (there is no UI message for this currently).
* The OTP is sent to the configured email address (by default this was set to the lecturer/marker email for testing). Check that email inbox after sending the OTP request.

**Important workflow note**

* Create accounts in this order to avoid access problems:

  1. Create at least one **Coordinator** and one **Manager** account for each Department you plan to test.
  2. Create the **Lecturer** account(s) that belong to that Department.

If you create a lecturer in a department but no Coordinator/Manager exists for that department, the lecturer’s claims cannot be approved by anyone in that department (since approval is department-scoped).

## Claim submission rules

* Lecturers **must** upload a supporting PDF document when submitting a claim. The server enforces:

  * File must be a **PDF** (extension `.pdf`).
  * Maximum file size **5 MB**.
  * If no file is supplied, the submission is rejected.
* Claims include: Hours Worked (decimal), Month (YYYY-MM), and the uploaded PDF.
* `TotalAmount` is calculated automatically as `HoursWorked * Department.HourlyRate` and stored in the `Claims` table.

## Coordinator / Manager responsibilities

* Coordinators and Managers see only claims associated with lecturers in **their department**.
* They can **Approve** or **Reject** claims. Rejection/approval sets the claim `Status` accordingly and can optionally include a comment (if implemented/seeded in your database schema).

## File storage

* Uploaded documents are stored under `wwwroot/uploads/` and referenced in the `SupportingDocuments` table via a relative path (e.g. `uploads/abcd.pdf`).
* Downloading a document streams it back from disk via the `Dashboard/DownloadDocument/{id}` action.

## Troubleshooting & common commands

* Migrations: `dotnet ef migrations add <Name>` and `dotnet ef database update` (only needed if you change models locally).
* If you see the `PendingModelChangesWarning` on startup: create a migration reflecting the model changes, then update the DB.
* If uploads fail: confirm `wwwroot/uploads` exists and the app has write permissions.
* If download returns 404: check the `SupportingDocuments.FilePath` value and that the physical file exists under `wwwroot/uploads`.

## Notes for marker / lecturer

* To test OTP behavior, request an OTP when registering a Coordinator/Manager and check the designated email inbox used in the project.
* The app includes automatic DB creation when migrations are present. If your lecturer plans to run the project, ensure the `Migrations` folder is included so the DB builds automatically.
