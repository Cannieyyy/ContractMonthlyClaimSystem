# Contract Monthly Claim System

A prototype web application for managing contract lecturer claims within a school or university environment.  
This system allows **lecturers, coordinators, and managers** to manage claims efficiently, with role-based dashboards.

---

##  Features

###  Lecturer
- Submit claims for hours worked.  
- Track previously submitted claims.  
- View analytics and claim history.  

###  Coordinator
- Verify lecturer claims.  
- Manage lecturers in their department.  
- Generate reports for departmental claims.  

###  Academic Manager
- Verify verified claims.  
- Oversee coordinators and lecturers.  
- Access consolidated reports.  

---

##  Tech Stack
- **Framework:** ASP.NET Core 8 MVC  
- **Frontend:** Bootstrap 5, Custom CSS, JavaScript  
- **Charts & Visuals:** Chart.js  
- **Database:** SQL Server (planned integration)  

---

##  Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  
- [SQL Server](https://www.microsoft.com/en-us/sql-server) *(future backend integration)*  
- Visual Studio 2022 or Visual Studio Code  

### Installation & Running

Follow these steps to run the project locally:

1. **Clone the repository**
   ```bash
   git clone https://github.com/<your-username>/ContractMonthlyClaimSystem.git
   cd ContractMonthlyClaimSystem
Restore dependencies

bash
Copy code
dotnet restore
Build the project

bash
Copy code
dotnet build
Run the application

bash
Copy code
dotnet run
Access in browser
Open your browser and navigate to:

arduino
Copy code
https://localhost:5001
