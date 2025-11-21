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
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            if (empId == null) return RedirectToAction("Login_Register", "Home");

            // Load employee (optional)
            var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (employee == null) return RedirectToAction("Login_Register", "Home");

            // Important: include SupportingDocuments
            var claims = await _db.Claims
                .Where(c => c.EmployeeID == empId.Value)
                .Include(c => c.SupportingDocuments)    // <--- this ensures docs appear
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

            if (!role.Equals("Coordinator", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var user = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            var claim = await _db.Claims
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.ClaimID == claimId);

            if (claim == null)
                return Json(new { success = false, message = "Claim not found." });

            // Only act inside same department
            if (claim.Employee.DepartmentID != user.DepartmentID)
                return Json(new { success = false, message = "You cannot act on claims outside your department." });

            // ❌ BLOCK actions on Deleted
            if (claim.Status.Equals("Deleted", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = false, message = "Deleted claims cannot be verified." });

            // ❌ BLOCK second verification
            if (claim.Status.Equals("Verified", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = false, message = "This claim is already verified." });

            // ❌ BLOCK verification if approved
            if (claim.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = false, message = "Approved claims cannot be verified." });

            // PASS — verify the claim
            claim.Status = "Verified";
            _db.Claims.Update(claim);

            _db.Verifications.Add(new Verification
            {
                ClaimID = claim.ClaimID,
                VerifiedBy = user.EmployeeID,
                VerificationDate = DateTime.UtcNow,
                Status = "Verified",
                Remarks = null
            });

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Claim Verified." });
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

            if (!role.Equals("Coordinator", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var user = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            var claim = await _db.Claims
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.ClaimID == claimId);

            if (claim == null)
                return Json(new { success = false, message = "Claim not found." });



            if (claim.Employee.DepartmentID != user.DepartmentID)
                return Json(new { success = false, message = "You cannot act on claims outside your department." });

            string status = claim.Status?.Trim().ToLower() ?? "";

            //BLOCK rejecting verified claims
            if (status.Equals("verified"))
                return Json(new { success = false, message = "Verified claims cannot be rejected." });

            
            if (status == "approved")
                return Json(new { success = false, message = "Approved claims cannot be rejected." });

            //BLOCK rejecting deleted claims
            if (status == "deleted")
                return Json(new { success = false, message = "Deleted claims cannot be rejected." });

            // PASS — reject the claim
            claim.Status = "Rejected";
            _db.Claims.Update(claim);

            var remarks = string.IsNullOrWhiteSpace(comment) ? null : comment;

            _db.Verifications.Add(new Verification
            {
                ClaimID = claim.ClaimID,
                VerifiedBy = user.EmployeeID,
                VerificationDate = DateTime.UtcNow,
                Status = "Rejected",
                Remarks = remarks
            });

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Claim rejected." });
        }



        // Small helper: redirect to the application's download endpoint
        public IActionResult Download(int documentId)
        {
            return RedirectToAction("DownloadDocument", "Dashboard", new { id = documentId });
        }
    }
}