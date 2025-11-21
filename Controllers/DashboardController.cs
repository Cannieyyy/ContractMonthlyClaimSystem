using System.Diagnostics;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;




namespace ContractMonthlyClaimSystem.Controllers
{
    
        public class DashboardController : Controller
        {
        private readonly ILogger<DashboardController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public DashboardController(ILogger<DashboardController> logger, IWebHostEnvironment env, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
            _env = env;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(IFormCollection form, IFormFile? supportingDoc)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            if (empId == null)
            {
                TempData["ClaimError"] = "You must be logged in to submit a claim.";
                return RedirectToAction("Lecture");
            }

            // Parse hours
            if (!decimal.TryParse(form["hoursWorked"], out var hoursWorked))
            {
                TempData["ClaimError"] = "Please enter valid hours worked.";
                return RedirectToAction("Lecture");
            }

            // Parse month (input type="month" returns "YYYY-MM")
            var monthValue = form["workMonth"].ToString();
            if (string.IsNullOrWhiteSpace(monthValue) || !DateTime.TryParse(monthValue + "-01", out var claimDate))
            {
                TempData["ClaimError"] = "Please select a valid month.";
                return RedirectToAction("Lecture");
            }

            // ***** NEW: Require supporting document *****
            if (supportingDoc == null || supportingDoc.Length == 0)
            {
                TempData["ClaimError"] = "A supporting PDF document is required to submit a claim.";
                return RedirectToAction("Lecture");
            }

            // ***** NEW: Validate file size (max 5 MB) *****
            const long maxBytes = 5 * 1024 * 1024; // 5MB
            if (supportingDoc.Length > maxBytes)
            {
                TempData["ClaimError"] = "Supporting document is too large. Maximum allowed size is 5 MB.";
                return RedirectToAction("Lecture");
            }

            // ***** NEW: Validate extension and content type (PDF) *****
            var allowedExt = ".pdf";
            var ext = Path.GetExtension(supportingDoc.FileName) ?? "";
            if (!string.Equals(ext, allowedExt, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ClaimError"] = "Supporting document must be a PDF file (.pdf).";
                return RedirectToAction("Lecture");
            }

            // Some clients may not set content-type reliably; be defensive.
            var contentType = supportingDoc.ContentType ?? "";
            if (!contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) &&
                !contentType.StartsWith("application/") /* allow some server variation but still check ext */)
            {
                // still allow if extension is .pdf, but warn if content-type is not pdf
                // If you want to strictly enforce content-type, uncomment the lines below:
                // TempData["ClaimError"] = "Supporting document must be a PDF (content type application/pdf).";
                // return RedirectToAction("Lecture");
            }

            // Lookup employee and department hourly rate
            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (employee == null)
            {
                TempData["ClaimError"] = "Employee not found.";
                return RedirectToAction("Lecture");
            }

            var department = await _db.Departments.FirstOrDefaultAsync(d => d.DepartmentID == employee.DepartmentID);
            if (department == null)
            {
                TempData["ClaimError"] = "Employee department not found.";
                return RedirectToAction("Lecture");
            }

            var totalAmount = Math.Round(hoursWorked * department.HourlyRate, 2);

            using var trx = await _db.Database.BeginTransactionAsync();
            try
            {
                var claim = new ContractMonthlyClaimSystem.Models.Claim
                {
                    EmployeeID = empId.Value,
                    HoursWorked = hoursWorked,
                    ClaimDate = claimDate,
                    Status = "Pending",
                    DateCreated = DateTime.UtcNow,
                    TotalAmount = totalAmount
                };

                _db.Claims.Add(claim);
                await _db.SaveChangesAsync(); // ClaimID available

                // Save file safely
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // sanitize and create unique filename
                var safeExt = ext.ToLowerInvariant();
                var fileName = $"{Guid.NewGuid():N}{safeExt}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await supportingDoc.CopyToAsync(stream);
                }

                var doc = new ContractMonthlyClaimSystem.Models.SupportingDocument
                {
                    ClaimID = claim.ClaimID,
                    FilePath = Path.Combine("uploads", fileName).Replace("\\", "/"),
                    UploadDate = DateTime.UtcNow
                };

                _db.SupportingDocuments.Add(doc);
                await _db.SaveChangesAsync();

                await trx.CommitAsync();
                TempData["ClaimSuccess"] = "Claim submitted successfully.";
                return RedirectToAction("Lecture");
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                _logger.LogError(ex, "SubmitClaim failed for EmployeeID={EmpId}", empId);
                TempData["ClaimError"] = "An error occurred while submitting the claim.";
                return RedirectToAction("Lecture");
            }
        }

        
        

        public async Task<IActionResult> Lecture()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            if (empId == null) return RedirectToAction("Login_Register", "Home");

            // Load employee (optional)
            var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (employee == null) return RedirectToAction("Login_Register", "Home");

            // Important: include SupportingDocuments
            var claims = await _db.Claims
                .Where(c => c.EmployeeID == empId.Value)
                .Include(c => c.SupportingDocuments)
                .Include(c => c.Verifications)   // Coordinator remarks
                .Include(c => c.Approvals)
                .OrderByDescending(c => c.DateCreated)
                .AsNoTracking()
                .ToListAsync();

            var vm = new CoordinatorDashboardViewModel
            {
                Claims = claims
            };

            ViewBag.TotalClaims = claims.Count;
            ViewBag.VerifiedClaims = claims.Count(c => c.Status == "Verified");
            ViewBag.ApprovedClaims = claims.Count(c => c.Status == "Approved");
            ViewBag.RejectedClaims = claims.Count(c => c.Status == "Rejected");


            return View(vm);
        }

        //download action
        public async Task<IActionResult> DownloadDocument(int id)
        {
            try
            {
                var doc = await _db.SupportingDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.DocumentID == id);
                if (doc == null)
                {
                    _logger.LogWarning("DownloadDocument: doc not found id={Id}", id);
                    return NotFound(); // 404
                }

                // Build full path from the stored FilePath
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                // Normalize stored path (ensure it doesn't start with a slash)
                var relativePath = doc.FilePath?.Trim().Replace('\\', '/').TrimStart('/');
                if (string.IsNullOrEmpty(relativePath))
                {
                    _logger.LogWarning("DownloadDocument: invalid FilePath for doc id={Id}", id);
                    return NotFound();
                }

                var fullPath = Path.Combine(webRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (!System.IO.File.Exists(fullPath))
                {
                    _logger.LogWarning("DownloadDocument: file missing on disk. id={Id}, path={Path}", id, fullPath);
                    return NotFound();
                }

                // Ensure it's a PDF (defensive)
                var contentType = "application/pdf";
                var fileName = Path.GetFileName(fullPath);

                // Return as an attachment so browsers download it
                var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return File(fs, contentType, fileDownloadName: fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DownloadDocument failed for id={Id}", id);
                return StatusCode(500, "An error occurred while trying to download the file.");
            }
        }

        public async Task<IActionResult> HRDashboard()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            if (empId == null)
                return RedirectToAction("Login_Register", "Home");

            // Check if logged user is HR Admin
            var employee = await _db.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);

            if (employee == null || employee.Role != "HR Admin")
                return RedirectToAction("Login_Register", "Home");

            // Load employees + departments
            var employees = await _db.Employees
                .Include(e => e.Department)
                .AsNoTracking()
                .ToListAsync();

            // Load the user accounts (where IsActive lives)
            var accounts = await _db.UserAccounts
                .AsNoTracking()
                .ToListAsync();

            // Attach activation status to each employee
            foreach (var emp in employees)
            {
                var acc = accounts.FirstOrDefault(a => a.EmployeeID == emp.EmployeeID);
                emp.IsActive = acc?.IsActive ?? false; 
            }

            var vm = new HRDashboardViewModel
            {
                Departments = await _db.Departments.AsNoTracking().ToListAsync(),
                Employees = employees
            };

            return View("HRDashboard", vm);
        }



        public async Task<IActionResult> ProgramCoordinator()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            if (empId == null) return RedirectToAction("Login_Register", "Home");

            // Load employee (optional)
            var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (employee == null) return RedirectToAction("Login_Register", "Home");

            var claims = await _db.Claims
    .Include(c => c.Employee) // needed for department
    .Include(c => c.SupportingDocuments)
    .Where(c => c.Employee.DepartmentID == employee.DepartmentID)
    .OrderByDescending(c => c.DateCreated)
    .AsNoTracking()
    .ToListAsync();

            var vm = new CoordinatorDashboardViewModel
            {
                Claims = claims
            };

            ViewBag.TotalClaims = claims.Count;
            ViewBag.VerifiedClaims = claims.Count(c => c.Status == "Verified");
            ViewBag.ApprovedClaims = claims.Count(c => c.Status == "Approved");
            ViewBag.RejectedClaims = claims.Count(c => c.Status == "Rejected");


            return View("ProgramCoordinator",vm);
        }

        public async Task<IActionResult> AcademicManager()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            if (empId == null) return RedirectToAction("Login_Register", "Home");

            // Load employee (optional)
            var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (employee == null) return RedirectToAction("Login_Register", "Home");

            // Important: include SupportingDocuments
            var claims = await _db.Claims
    .Include(c => c.Employee) // needed for DepartmentID
    .Include(c => c.SupportingDocuments)
    .Where(c => c.Employee.DepartmentID == employee.DepartmentID)
    .OrderByDescending(c => c.DateCreated)
    .AsNoTracking()
    .ToListAsync();


            var vm = new CoordinatorDashboardViewModel
            {
                Claims = claims
            };

            ViewBag.TotalClaims = claims.Count;
            ViewBag.VerifiedClaims = claims.Count(c => c.Status == "Verified");
            ViewBag.ApprovedClaims = claims.Count(c => c.Status == "Approved");
            ViewBag.RejectedClaims = claims.Count(c => c.Status == "Rejected");


            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClaim(int claimId)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            if (empId == null) return RedirectToAction("Login_Register", "Home");

            var claim = await _db.Claims.FirstOrDefaultAsync(c => c.ClaimID == claimId && c.EmployeeID == empId.Value);
            if (claim == null)
            {
                TempData["ClaimError"] = "Claim not found.";
                return RedirectToAction("Lecture");
            }

            // Soft-delete: mark status
            claim.Status = "Deleted";
            _db.Claims.Update(claim);
            await _db.SaveChangesAsync();

            TempData["ClaimSuccess"] = "Claim deleted.";
            return RedirectToAction("Lecture");
        }

        [HttpGet]
        public async Task<IActionResult> EditClaim(int id)
        {
            // Get the current logged-in employee
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            if (empId == null) return RedirectToAction("Login_Register", "Home");

            // Fetch the claim for this employee
            var claim = await _db.Claims
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClaimID == id && c.EmployeeID == empId.Value);

            if (claim == null) return NotFound();

            // Map claim to ViewModel for modal
            var vm = new ClaimEditViewModel
            {
                ClaimID = claim.ClaimID,
                HoursWorked = claim.HoursWorked,
                WorkMonth = claim.ClaimDate.ToString("yyyy-MM"),
                // optional: include Status if you want to show it in the modal
                Status = claim.Status
            };

            // Return JSON for modal population
            return Json(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClaim(ClaimEditViewModel model, IFormFile? NewSupportingDoc)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            if (empId == null) return RedirectToAction("Login_Register", "Home");

            // Manual decimal parse because culture breaks ModelState
            if (!decimal.TryParse(model.HoursWorked.ToString(),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var hours))
            {
                TempData["ClaimError"] = "Hours Worked must be a valid number.";
                return RedirectToAction("Lecture");
            }

            model.HoursWorked = hours;

            // ----- VALIDATE HOURS -----
            if (model.HoursWorked < 1)
            {
                TempData["ClaimError"] = "Hours Worked must be greater than 0.";
                return RedirectToAction("Lecture");
            }

            if (model.HoursWorked > 140)
            {
                TempData["ClaimError"] = "Hours Worked cannot exceed 140 hours.";
                return RedirectToAction("Lecture");
            }


            // Validate month format manually
            if (string.IsNullOrWhiteSpace(model.WorkMonth) ||
                !DateTime.TryParse(model.WorkMonth + "-01", out var parsedMonth))
            {
                TempData["ClaimError"] = "Please select a valid month.";
                return RedirectToAction("Lecture");
            }


            // Load claim with supporting docs
            var claim = await _db.Claims
                .Include(c => c.SupportingDocuments)
                .FirstOrDefaultAsync(c => c.ClaimID == model.ClaimID && c.EmployeeID == empId.Value);

            if (claim == null)
            {
                TempData["ClaimError"] = "Claim not found.";
                return RedirectToAction("Lecture");
            }

            // Parse new month
            if (!DateTime.TryParse(model.WorkMonth + "-01", out var claimDate))
            {
                TempData["ClaimError"] = "Invalid month format.";
                return RedirectToAction("Lecture");
            }

            // Get department hourly rate
            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (employee == null)
            {
                TempData["ClaimError"] = "Employee not found.";
                return RedirectToAction("Lecture");
            }
            var dept = await _db.Departments.FirstOrDefaultAsync(d => d.DepartmentID == employee.DepartmentID);
            if (dept == null)
            {
                TempData["ClaimError"] = "Department not found.";
                return RedirectToAction("Lecture");
            }

            // Update properties
            claim.HoursWorked = model.HoursWorked;
            claim.ClaimDate = claimDate;
            claim.TotalAmount = Math.Round(model.HoursWorked * dept.HourlyRate, 2);
            claim.Status = "Pending";


            // If a new file is uploaded, validate and replace
            if (NewSupportingDoc != null && NewSupportingDoc.Length > 0)
            {
                const long maxBytes = 5 * 1024 * 1024; // 5 MB
                var ext = Path.GetExtension(NewSupportingDoc.FileName) ?? "";
                if (!string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ClaimError"] = "Supporting document must be a PDF (.pdf).";
                    return RedirectToAction("Lecture");
                }
                if (NewSupportingDoc.Length > maxBytes)
                {
                    TempData["ClaimError"] = "Supporting document is too large. Max 5 MB.";
                    return RedirectToAction("Lecture");
                }

                using var trx = await _db.Database.BeginTransactionAsync();
                try
                {
                    // Save new file to disk
                    var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var newFileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
                    var fullNewPath = Path.Combine(uploadsFolder, newFileName);

                    using (var stream = new FileStream(fullNewPath, FileMode.Create))
                    {
                        await NewSupportingDoc.CopyToAsync(stream);
                    }

                    // Remove old doc and physical file if exists (delete DB row)
                    var oldDoc = claim.SupportingDocuments?.FirstOrDefault();
                    if (oldDoc != null)
                    {
                        // delete physical file if present
                        try
                        {
                            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                            var oldFullPath = Path.Combine(webRoot, oldDoc.FilePath.Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(oldFullPath))
                            {
                                System.IO.File.Delete(oldFullPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete old supporting file for DocumentID={DocId}", oldDoc.DocumentID);
                            // non-fatal — continue
                        }

                        // remove old DB row
                        _db.SupportingDocuments.Remove(oldDoc);
                        await _db.SaveChangesAsync();
                    }

                    // add new db doc
                    var doc = new SupportingDocument
                    {
                        ClaimID = claim.ClaimID,
                        FilePath = Path.Combine("uploads", newFileName).Replace("\\", "/"),
                        UploadDate = DateTime.UtcNow
                    };
                    _db.SupportingDocuments.Add(doc);

                    // update claim and save
                    _db.Claims.Update(claim);
                    await _db.SaveChangesAsync();

                    await trx.CommitAsync();

                    TempData["ClaimSuccess"] = "Claim updated (document replaced).";
                    return RedirectToAction("Lecture");
                }
                catch (Exception ex)
                {
                    await _db.Database.RollbackTransactionAsync();
                    _logger.LogError(ex, "EditClaim: failed replacing document for ClaimID={ClaimId}", claim.ClaimID);
                    TempData["ClaimError"] = "An error occurred while updating the claim document.";
                    return RedirectToAction("Lecture");
                }
            }
            else
            {
                // No new file uploaded — just update claim fields
                _db.Claims.Update(claim);
                await _db.SaveChangesAsync();
                TempData["ClaimSuccess"] = "Claim updated.";
                return RedirectToAction("Lecture");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    

}
