using System.IO.Compression;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;



namespace ContractMonthlyClaimSystem.Controllers
{
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HRController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HRDashboardViewModel
            {
                Departments = await _db.Departments.ToListAsync(),
                Employees = await _db.Employees.Include(e => e.Department).ToListAsync()
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> GetFilteredClaims(
    int? departmentId,
    string? status,
    int? month,
    string? lecturer)
        {
            try
            {
                var query = _db.Claims
                    .Include(c => c.Employee)
                        .ThenInclude(e => e.Department)
                    .Include(c => c.SupportingDocuments)
                    .AsNoTracking()
                    .AsQueryable();

                // Filter: Department
                if (departmentId.HasValue && departmentId.Value > 0)
                {
                    query = query.Where(c => c.Employee.DepartmentID == departmentId.Value);
                }

                // Filter: Status
                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(c => c.Status == status);
                }

                // Filter: Month
                if (month.HasValue && month.Value >= 1 && month.Value <= 12)
                {
                    query = query.Where(c => c.ClaimDate.Month == month.Value);
                }

                // Filter: Lecturer Name
                if (!string.IsNullOrWhiteSpace(lecturer))
                {
                    query = query.Where(c => c.Employee.Name.Contains(lecturer));
                }

                var claims = await query
                    .OrderByDescending(c => c.DateCreated)
                    .ToListAsync();

                var output = claims.Select(c => new
                {
                    claimID = c.ClaimID,
                    lecturerName = c.Employee?.Name,
                    departmentName = c.Employee?.Department?.Name,
                    hoursWorked = c.HoursWorked,
                    claimDate = c.ClaimDate.ToString("yyyy-MM-dd"),
                    totalAmount = c.TotalAmount,
                    status = c.Status,
                    documentID = c.SupportingDocuments?.FirstOrDefault()?.DocumentID
                });

                return Json(new { success = true, data = output });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Server error: " + ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoice(int id)
        {
            try
            {
                var claim = await _db.Claims
                    .Include(c => c.Employee)
                        .ThenInclude(e => e.Department)
                    .Include(c => c.SupportingDocuments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.ClaimID == id);

                if (claim == null)
                {
                    return Json(new { success = false, message = "Claim not found" });
                }

                var doc = claim.SupportingDocuments?.FirstOrDefault();

                var invoiceDto = new
                {
                    claimID = claim.ClaimID,
                    lecturerName = claim.Employee?.Name,
                    departmentName = claim.Employee?.Department?.Name,
                    claimDate = claim.ClaimDate.ToString("yyyy-MM-dd"),
                    hoursWorked = claim.HoursWorked,
                    hourlyRate = claim.Employee?.Department?.HourlyRate ?? 0,
                    totalAmount = claim.TotalAmount,
                    documentID = doc?.DocumentID
                };

                return Json(new { success = true, data = invoiceDto });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> DownloadInvoicePDF(int id)
        {
            var claim = await _db.Claims
                .Include(c => c.Employee)
                    .ThenInclude(e => e.Department)
                .Include(c => c.SupportingDocuments) // optional if you want docs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClaimID == id);

            if (claim == null)
                return NotFound();

            // Use your modern invoice generator
            var pdfBytes = GenerateInvoicePdfBytes(claim);

            return File(pdfBytes, "application/pdf", $"Invoice_{claim.ClaimID}.pdf");
        }


        private byte[] GenerateInvoicePdfBytes(Claim claim)
        {
            if (claim == null) throw new ArgumentNullException(nameof(claim));

            var employee = claim.Employee;
            var dept = employee?.Department;
            var saCulture = new System.Globalization.CultureInfo("en-ZA");

            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // ==== Colors ====
                var darkGrey = new BaseColor(30, 30, 30);
                var lightGrey = new BaseColor(230, 230, 230);
                var black = BaseColor.Black;

                // ==== Fonts ====
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, darkGrey);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, darkGrey);
                var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, darkGrey);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, black);
                var italicFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10, black);
                var totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, darkGrey);

                // ==== Header Section (unchanged) ====
                var headerTable = new PdfPTable(2) { WidthPercentage = 100 };
                headerTable.SetWidths(new float[] { 60f, 40f });

                // Left: Company Name + Employee Info
                var companyCell = new PdfPCell();
                companyCell.Border = Rectangle.NO_BORDER;
                companyCell.AddElement(new Paragraph("Time2Pay", titleFont));
                companyCell.AddElement(new Paragraph($"{employee?.Name ?? "—"}", italicFont));
                companyCell.AddElement(new Paragraph($"{employee?.Email ?? "—"}", italicFont));
                companyCell.AddElement(new Paragraph($"{dept?.Name ?? "—"}", italicFont));
                headerTable.AddCell(companyCell);

                // Right: Invoice # and Date
                var invoiceCell = new PdfPCell();
                invoiceCell.BackgroundColor = lightGrey;
                invoiceCell.Padding = 10;
                invoiceCell.Border = Rectangle.NO_BORDER;
                invoiceCell.AddElement(new Paragraph($"INVOICE #{claim.ClaimID}", headerFont));
                invoiceCell.AddElement(new Paragraph($"Date: {claim.ClaimDate:yyyy-MM-dd}", normalFont));
                headerTable.AddCell(invoiceCell);

                doc.Add(headerTable);
                doc.Add(new Paragraph("\n"));

                // ==== Claim Details Table ====
                var claimTable = new PdfPTable(1) { WidthPercentage = 100, HorizontalAlignment = Element.ALIGN_LEFT };
                claimTable.SetWidths(new float[] { 2f }); // Single column table
                claimTable.DefaultCell.Border = Rectangle.BOX;
                claimTable.DefaultCell.BackgroundColor = lightGrey;
                claimTable.DefaultCell.Padding = 6;

                // Top row: Header (Claim Details)
                var headerCellClaim = new PdfPCell(new Phrase("Claim Details", sectionFont))
                {
                    BackgroundColor = lightGrey,
                    Border = Rectangle.BOX,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingBottom = 6
                };
                claimTable.AddCell(headerCellClaim);

                // Next row: Hours Worked
                var hoursCell = new PdfPCell(new Phrase($"Hours Worked: {claim.HoursWorked}", normalFont))
                {
                    Border = Rectangle.LEFT_BORDER + Rectangle.RIGHT_BORDER,
                    BackgroundColor = lightGrey,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingBottom = 2
                };
                claimTable.AddCell(hoursCell);

                // Next row: Hourly Rate
                var rateCell = new PdfPCell(new Phrase($"Hourly Rate: {(dept?.HourlyRate ?? 0m).ToString("C", saCulture)}", normalFont))
                {
                    Border = Rectangle.LEFT_BORDER + Rectangle.RIGHT_BORDER + Rectangle.BOTTOM_BORDER,
                    BackgroundColor = lightGrey,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingBottom = 4
                };
                claimTable.AddCell(rateCell);

                doc.Add(claimTable);
                doc.Add(new Paragraph("\n"));

                // ==== Total Amount Section ====
                var totalTable = new PdfPTable(2) { WidthPercentage = 40, HorizontalAlignment = Element.ALIGN_RIGHT };
                totalTable.SetWidths(new float[] { 60f, 40f });

                var totalLabelCell = new PdfPCell(new Phrase("Total Amount:", normalFont))
                {
                    BackgroundColor = lightGrey,
                    Border = Rectangle.NO_BORDER,
                    Padding = 4
                };
                var totalValueCell = new PdfPCell(new Phrase(claim.TotalAmount.ToString("C", saCulture), totalFont))
                {
                    BackgroundColor = lightGrey,
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 4
                };
                totalTable.AddCell(totalLabelCell);
                totalTable.AddCell(totalValueCell);
                doc.Add(totalTable);

                // ==== Footer / Signature ====
                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph("Authorized by: ____________________", normalFont) { SpacingBefore = 20f });
                doc.Add(new Paragraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} (UTC)", normalFont));

                doc.Close();
                return ms.ToArray();
            }
        }









        [HttpGet]
        public async Task<IActionResult> ExportBatchPDF(int? departmentId, string? status, int? month, string? lecturer)
        {
            // Load claims exactly like GetFilteredClaims
            var query = _db.Claims
                .Include(c => c.Employee).ThenInclude(e => e.Department)
                .AsNoTracking()
                .AsQueryable();

            if (departmentId.HasValue && departmentId.Value > 0)
                query = query.Where(c => c.Employee.DepartmentID == departmentId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(c => c.Status == status);

            if (month.HasValue && month.Value >= 1 && month.Value <= 12)
                query = query.Where(c => c.ClaimDate.Month == month.Value);

            if (!string.IsNullOrWhiteSpace(lecturer))
                query = query.Where(c => c.Employee.Name.Contains(lecturer));

            var claims = await query.ToListAsync();

            if (!claims.Any())
                return BadRequest("No invoices found for selected filters.");

            using (var zipStream = new MemoryStream())
            {
                using (var zip = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (var claim in claims)
                    {
                        var pdfBytes = GenerateInvoicePdfBytes(claim);

                        var zipEntry = zip.CreateEntry($"Invoice_{claim.ClaimID}.pdf");

                        using var entryStream = zipEntry.Open();
                        entryStream.Write(pdfBytes, 0, pdfBytes.Length);
                    }
                }

                return File(zipStream.ToArray(), "application/zip", "Filtered_Invoices.zip");
            }
        }



    }
}
