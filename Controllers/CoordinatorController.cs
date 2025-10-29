using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CoordinatorController> _logger;

        public CoordinatorController(ApplicationDbContext db, IWebHostEnvironment env, ILogger<CoordinatorController> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        // GET: /Coordinator
        public async Task<IActionResult> Index()
        {
            // Get coordinator's department (assuming stored in session)
            var deptId = HttpContext.Session.GetInt32("DepartmentID");

            // Query claims for the coordinator's department (exclude deleted)
            var claimsQuery = _db.Claims
                .Include(c => c.Employee)
                .Include(c => c.SupportingDocuments) // optional, if view needs documents
                .Where(c => !c.IsDeleted);

            if (deptId.HasValue)
                claimsQuery = claimsQuery.Where(c => c.Employee.DepartmentID == deptId.Value);

            var claims = await claimsQuery
                .OrderByDescending(c => c.DateCreated)
                .AsNoTracking()
                .ToListAsync();

            // Build view model
            var vm = new CoordinatorDashboardViewModel
            {
                PendingCount = claims.Count(c => c.Status == "Pending"),
                InProgress = claims.Count(c => c.Status == "In Progress"),
                VerifiedCount = claims.Count(c => c.Status == "Verified"),
                RejectedCount = claims.Count(c => c.Status == "Rejected"),
                Claims = claims
            };

            return View(vm);

        }

        [HttpGet]
        public async Task<IActionResult> GetClaimsForVerification()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            var role = HttpContext.Session.GetString("Role");
            if (empId == null || string.IsNullOrEmpty(role))
                return Json(new { success = false, message = "Not authenticated." });

            if (!role.Equals("Coordinator", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var coordinator = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (coordinator == null)
                return Json(new { success = false, message = "Coordinator record not found." });

            var deptId = coordinator.DepartmentID;

            var claims = await _db.Claims
                .Include(c => c.Employee)
                .Include(c => c.SupportingDocuments)
                .Where(c => c.Employee.DepartmentID == deptId)
                .OrderByDescending(c => c.DateCreated)
                .AsNoTracking()
                .ToListAsync();

            var dto = claims.Select(c => new ClaimForVerificationDto
            {
                ClaimID = c.ClaimID,
                EmployeeID = c.EmployeeID,
                EmployeeName = c.Employee?.Name ?? "",
                ClaimDate = c.ClaimDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                WorkMonth = c.ClaimDate.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                HoursWorked = c.HoursWorked,
                TotalAmount = c.TotalAmount,
                Status = c.Status,
                DocumentID = c.SupportingDocuments?.FirstOrDefault()?.DocumentID,
                DocumentName = c.SupportingDocuments?.FirstOrDefault()?.FilePath
            }).ToList();

            return Json(new { success = true, data = dto });
        }


        // POST: /Coordinator/VerifyClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyClaim(int claimId)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            var role = HttpContext.Session.GetString("Role");
            if (empId == null || string.IsNullOrEmpty(role))
                return Json(new { success = false, message = "Not authenticated." });

            if (!role.Equals("Coordinator", StringComparison.OrdinalIgnoreCase) && !role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var coordinator = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (coordinator == null)
                return Json(new { success = false, message = "Coordinator record not found." });

            var claim = await _db.Claims
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.ClaimID == claimId);

            if (claim == null)
                return NotFound(new { success = false, message = "Claim not found." });

            // Only claims that belong to the coordinator's department are actionable
            var claimEmployee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeID == claim.EmployeeID);
            if (claimEmployee == null || claimEmployee.DepartmentID != coordinator.DepartmentID)
                return Json(new { success = false, message = "You cannot act on claims outside your department." });

            if (!string.Equals(claim.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
            {
                claim.Status = "Verified";
                _db.Claims.Update(claim);

                // Add verification record
                var verification = new Verification
                {
                    ClaimID = claim.ClaimID,
                    VerifiedBy = coordinator.EmployeeID,
                    VerificationDate = DateTime.UtcNow,
                    Status = "Verified",
                    Remarks = null // optional, or pass comment if you want
                };
                _db.Verifications.Add(verification);

                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Claim Verified." });
            }

            else
            {
                return BadRequest(new { success = false, message = "Deleted claims cannot be Verified." });
            }
        }

        // POST: /Coordinator/RejectClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int claimId, string? comment)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            var role = HttpContext.Session.GetString("Role");
            if (empId == null || string.IsNullOrEmpty(role))
                return Json(new { success = false, message = "Not authenticated." });

            if (!role.Equals("Coordinator", StringComparison.OrdinalIgnoreCase) && !role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var coordinator = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (coordinator == null)
                return Json(new { success = false, message = "Coordinator record not found." });

            var claim = await _db.Claims
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.ClaimID == claimId);

            if (claim == null)
                return NotFound(new { success = false, message = "Claim not found." });

            // Only claims in same department
            var claimEmployee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeID == claim.EmployeeID);
            if (claimEmployee == null || claimEmployee.DepartmentID != coordinator.DepartmentID)
                return Json(new { success = false, message = "You cannot act on claims outside your department." });

            if (!string.Equals(claim.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
            {
                claim.Status = "Rejected";

                // Optional comment
                string? remarks = !string.IsNullOrWhiteSpace(comment) ? comment : null;
                claim.GetType().GetProperty("CoordinatorComment")?.SetValue(claim, remarks);

                _db.Claims.Update(claim);

                // Add verification record
                var verification = new Verification
                {
                    ClaimID = claim.ClaimID,
                    VerifiedBy = coordinator.EmployeeID,
                    VerificationDate = DateTime.UtcNow,
                    Status = "Rejected",
                    Remarks = remarks
                };
                _db.Verifications.Add(verification);

                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Claim rejected." });
            }

            else
            {
                return BadRequest(new { success = false, message = "Deleted claims cannot be rejected." });
            }
        }

        // Small helper: redirect to the application's download endpoint
        public IActionResult Download(int documentId)
        {
            return RedirectToAction("DownloadDocument", "Dashboard", new { id = documentId });
        }
    }
}