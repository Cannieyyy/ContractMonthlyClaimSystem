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
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(ApplicationDbContext db, IWebHostEnvironment env, ILogger<ManagerController> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        // GET: /Manager
        public async Task<IActionResult> Index()
        {
            // Similar to Coordinator.Index: load claims for display (non-deleted)
            var deptId = HttpContext.Session.GetInt32("DepartmentID");

            var claimsQuery = _db.Claims
                .Include(c => c.Employee)
                .Include(c => c.SupportingDocuments)
                .Where(c => !c.IsDeleted);

            if (deptId.HasValue)
                claimsQuery = claimsQuery.Where(c => c.Employee.DepartmentID == deptId.Value);

            var claims = await claimsQuery
                .OrderByDescending(c => c.DateCreated)
                .AsNoTracking()
                .ToListAsync();

            // You can pass a minimal model or ViewBag; the view you pasted doesn't require a model,
            // but returning View() will render the view.
            return View();
        }

        // GET: /Manager/GetClaimsForApproval
        [HttpGet]
        public async Task<IActionResult> GetClaimsForApproval()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            var role = HttpContext.Session.GetString("Role");
            if (empId == null || string.IsNullOrEmpty(role))
                return Json(new { success = false, message = "Not authenticated." });

            if (!role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var manager = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (manager == null)
                return Json(new { success = false, message = "Manager record not found." });

            var deptId = manager.DepartmentID;

            var claims = await _db.Claims
                .Include(c => c.Employee)
                .Include(c => c.SupportingDocuments)
                .Where(c => c.Employee.DepartmentID == deptId && !c.IsDeleted)
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

        // POST: /Manager/ApproveClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int claimId, string? comment)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeID");
            var role = HttpContext.Session.GetString("Role");
            if (empId == null || string.IsNullOrEmpty(role))
                return Json(new { success = false, message = "Not authenticated." });

            if (!role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var manager = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeID == empId.Value);
            if (manager == null)
                return Json(new { success = false, message = "Manager record not found." });

            var claim = await _db.Claims
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.ClaimID == claimId);

            if (claim == null)
                return NotFound(new { success = false, message = "Claim not found." });

            // Ensure claim belongs to manager's department
            var claimEmployee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeID == claim.EmployeeID);
            if (claimEmployee == null || claimEmployee.DepartmentID != manager.DepartmentID)
                return Json(new { success = false, message = "You cannot act on claims outside your department." });

            if (!string.Equals(claim.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
            {
                // Update claim status to Approved
                claim.Status = "Approved";
                _db.Claims.Update(claim);

                // Create an Approval record (table: Approval)
                var approval = new Approval
                {
                    ClaimID = claim.ClaimID,
                    ApprovedBy = manager.EmployeeID,
                    ApprovalDate = DateTime.UtcNow,
                    Status = "Approved",
                    Remarks = string.IsNullOrWhiteSpace(comment) ? null : comment
                };

                _db.Set<Approval>().Add(approval); // Assumes Approval model and DbSet<Approval> exist

                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Claim approved." });
            }
            else
            {
                return BadRequest(new { success = false, message = "Deleted claims cannot be approved." });
            }
        }
    }
}
