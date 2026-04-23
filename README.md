# 📘 Time2Pay – Claims Management System
*A PROG6212 POE Project – Developed by ST10459862 (Kopano Leshope)*

Time2Pay is a web-based claims management system designed to streamline the submission, verification, approval, and reporting of lecturer claims.  
The system delivers a consistent and simple user experience while enforcing strong validation, automation, and role-based access control.



# 📌 Table of Contents
- Overview  
- Key Features  
- User Roles & Functionalities  
- System Improvements  
- Automation & Validations  
- Technology Stack  
- Installation  


# 📖 Overview

Time2Pay provides a simple, consistent, and user-friendly interface.  
Every dashboard uses the same layout, including:

- Clean design across all pages  
- Feature action cards  
- Navigation bar  
- Quick stats showing:
  - Submitted claims  
  - Approved claims  
  - Verified claims  
  - Rejected claims  

This makes the platform easy to use because every page feels familiar and intuitive.



# ⭐ Key Features

### 🔹 Consistent User Interface
- Same layout across Lecturer, Coordinator, Manager, and HR dashboards  
- Clear navigation and feature cards  
- Instant feedback with notifications  

### 🔹 Automated Calculations
- Total Amount automatically calculated using Hours Worked × Department Rate  
- Report pages automatically compute totals  
- Invoices automatically generated using filters  

### 🔹 Strong Validations
- Hours worked must be between **1 and 140**  
- Approved claims **cannot** be edited or deleted  
- Coordinators cannot reject verified/approved claims  
- Managers cannot reject approved claims  
- Editing a rejected claim resets status to **Pending**  

### 🔹 Role-Based Access
- Each user sees claims only from **their assigned department**  
- HR has full system access  



# 🧑‍🏫 User Roles & Functionalities



## 👨‍🏫 Lecturer Dashboard

### ✔ Claim Submission
- Submit monthly hours worked  
- Auto-calculation of total amount  
- Validation:
  - Cannot submit 0 or negative hours  
  - Cannot submit more than 140 hours  
  - Clear error messages

### ✔ Claim Management
- Edit claims in **Pending** or **Verified** state  
- Approved claims are locked  
- Editing a rejected claim changes status to **Pending**

### ✔ Rejected Claim Comments
- Button shows the reason the claim was rejected  



## 🧑‍💼 Coordinator Dashboard

### ✔ Department-Only Access
- Can only see claims from lecturers in **their assigned department**

### ✔ Verification
- Can verify or reject claims  
- Cannot reject:
  - Verified claims  
  - Approved claims

### ✔ Reporting
- View approved claims for their department  
- Totals automatically calculated  



## 👨‍💼 Manager Dashboard

### ✔ Department-Only Access
- Can only see claims from **their department**

### ✔ Approval
- Can approve or reject claims  
- Cannot reject approved claims  

### ✔ Reporting
- Full report of approved claims  
- Automatically calculated totals  



## 🧑‍💼💼 HR Admin Dashboard (Top-Level Access)

### ✔ Monthly Invoice Generation
- Filter by department, lecturer, or month  
- Generate:
  - Single lecturer invoice  
  - Department batch invoice  
  - Full institution invoice  
- Export invoices as **PDF** (single or batch)

### ✔ Reports
- Generate reports using filters  
- System automatically calculates totals  

### ✔ User Account Management
- Change an employee’s department  
- Change employee roles  
- Activate or deactivate user accounts  



# 🔧 System Improvements

### Registration & Login
- Added successful login notifications  
- Added failure messages  
- Improved user feedback on registration

### Lecturer View
- Validation added for hours worked (1–140)  
- Prevents invalid claim submissions  
- Rejected claims show comment  
- Editing rejected claims resets status to **Pending**

### Coordinator & Manager
- Dynamic quick stats  
- Validations prevent rejecting verified/approved claims  
- Improved workflow consistency

### HR Admin
- Added invoice generator  
- Added batch PDF export  
- Added user account management panel  



# 🤖 Automation & Validations

The system automatically:

- Calculates claim totals  
- Prevents editing or deleting approved claims  
- Prevents rejecting verified or approved claims  
- Resets rejected claims to pending when edited  
- Filters claims based on user role  
- Generates PDF invoices  
- Calculates totals for reports  



# 🛠 Technology Stack

- ASP.NET Core MVC  
- C# / Entity Framework Core  
- SQL Server  
- JavaScript / Bootstrap 5  
- HTML / CSS  
- PDF Generation Library  



# 📦 Installation

### 1. Clone the repository
```bash
git clone https://github.com/your-repo/time2pay.git
