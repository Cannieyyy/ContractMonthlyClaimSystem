// Controllers/HomeController.cs
using System.Diagnostics;
using System.Net.Mail;
using System.Threading.Tasks;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Infrastructure;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly PasswordHasher<Employee> _passwordHasher;
        private readonly IMemoryCache _cache;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger,
                      ApplicationDbContext db,
                      IMemoryCache cache,
                      IEmailSender emailSender,
                      IConfiguration configuration)
        {
            _logger = logger;
            _db = db;
            _cache = cache;
            _emailSender = emailSender;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<Employee>();
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/Login_Register
        // This returns the view that contains both login and register forms.
        // We populate departments for the register dropdown here.
        [HttpGet]
        public async Task<IActionResult> Login_Register()
        {
            ViewBag.Departments = await _db.Departments.ToListAsync();

            return View(new RegisterViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOtp([FromForm] string email, [FromForm] int? departmentId)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            // generate 4-digit OTP
            var rng = new Random();
            var code = rng.Next(1000, 9999).ToString("D4");

            var key = $"otp:{email.ToLowerInvariant()}";
            var expiryMinutes = _configuration.GetValue<int>("Registration:OtpExpiryMinutes", 15);

            var otpEntry = new
            {
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                DepartmentId = departmentId
            };

            try
            {
                _cache.Set(key, otpEntry, TimeSpan.FromMinutes(expiryMinutes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set OTP in cache.");
                // return a 500 with a developer-friendly message for debugging
                return StatusCode(500, "Failed to store OTP in server cache: " + ex.Message);
            }

            var lecturerEmail = _configuration.GetValue<string>("Management:LecturerEmail");
            if (string.IsNullOrWhiteSpace(lecturerEmail))
            {
                _logger.LogError("Management:LecturerEmail not configured.");
                return StatusCode(500, "Management email not configured on server.");
            }

            var subject = "Time2Pay OTP Request";
            var body = $@"
                        <p>A registration requested an OTP.</p>
                        <p><strong>Requester email:</strong> {email}</p>
                        <p><strong>Department ID:</strong> {departmentId}</p>
                        <p><strong>OTP:</strong> <b>{code}</b></p>
                        <p>This code expires in {expiryMinutes} minutes.</p>
                    ";

            try
            {
                await _emailSender.SendEmailAsync(lecturerEmail, subject, body);
                return Ok(new { message = "OTP sent to lecturer." });
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP send failure.");
                // helpful message for dev: include SMTP error
                return StatusCode(500, "Failed to send OTP email (SMTP error): " + smtpEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending OTP.");
                return StatusCode(500, "Failed to send OTP email: " + ex.Message);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.Departments = await _db.Departments.ToListAsync();
            // HOTFIX: if role is Lecturer, remove any ModelState errors for OtpCode so lecturer can register
            if (string.Equals(model?.Role, "Lecturer", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.Remove(nameof(model.OtpCode)); // removes server-side validation error
                                                          // sometimes key might be "OtpCode" without model prefix; ensure both:
                ModelState.Remove("OtpCode");
            }

            // Clear previous messages
            TempData.Remove("RegisterError");
            TempData.Remove("RegisterDebug");

            // 1) Basic modelstate check
            if (!ModelState.IsValid)
            {
                var errs = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage + (e.Exception != null ? " (" + e.Exception.Message + ")" : "")));
                _logger.LogWarning("ModelState invalid on Register: {Errors}", errs);
                TempData["RegisterError"] = "Validation failed: " + (errs.Length > 0 ? errs : "see form errors.");
                return View("Login_Register", model);
            }

            // determine privileged role
            var isPrivileged = !string.Equals(model.Role, "Lecturer", StringComparison.OrdinalIgnoreCase);
            TempData["RegisterDebug"] = $"Role='{model.Role}', IsPrivileged={isPrivileged}";

            // 2) OTP validation for privileged roles
            if (isPrivileged)
            {
                if (string.IsNullOrWhiteSpace(model.OtpCode))
                {
                    TempData["RegisterError"] = "OTP is required for privileged roles. (No OTP provided)";
                    _logger.LogInformation("Register blocked: no OTP provided for privileged role.");
                    return View("Login_Register", model);
                }

                var key = $"otp:{model.Email?.ToLowerInvariant()}";
                if (!_cache.TryGetValue(key, out dynamic? otpEntry))
                {
                    TempData["RegisterError"] = "OTP not found or expired for this email. Please request a new OTP.";
                    _logger.LogInformation("Register blocked: OTP cache miss for key {Key}", key);
                    return View("Login_Register", model);
                }

                string cachedCode = otpEntry.Code as string ?? string.Empty;
                DateTime expiresAt = otpEntry.ExpiresAt;
                TempData["RegisterDebug"] += $" | CachedCode={cachedCode} | ExpiresAt={expiresAt:o}";

                if (DateTime.UtcNow > expiresAt)
                {
                    _cache.Remove(key);
                    TempData["RegisterError"] = "OTP expired. Please request a new OTP.";
                    _logger.LogInformation("Register blocked: OTP expired for key {Key}", key);
                    return View("Login_Register", model);
                }

                if (!string.Equals(cachedCode, model.OtpCode.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    TempData["RegisterError"] = "OTP does not match. Please check the code and try again.";
                    _logger.LogInformation("Register blocked: OTP mismatch for key {Key} (provided {Provided})", key, model.OtpCode);
                    return View("Login_Register", model);
                }

                // OTP valid: remove it so it can't be reused
                _cache.Remove(key);
                TempData["RegisterDebug"] += " | OTP validated and removed from cache.";
            }

            // 3) Duplicate email check
            var existing = await _db.Employees.FirstOrDefaultAsync(e => e.Email == model.Email);
            if (existing != null)
            {
                TempData["RegisterError"] = "Email already in use.";
                _logger.LogInformation("Register blocked: duplicate email {Email}", model.Email);
                return View("Login_Register", model);
            }

            // 4) Attempt DB save inside transaction
            using var trx = await _db.Database.BeginTransactionAsync();
            try
            {
                var employee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    DepartmentID = model.DepartmentID,
                    Role = model.Role,
                    DateCreated = DateTime.UtcNow
                };

                _db.Employees.Add(employee);

                var countEmp = await _db.SaveChangesAsync();
                _logger.LogInformation("Employees saved: {CountEmp}, EmployeeID: {EmpId}", countEmp, employee.EmployeeID);

                var userAccount = new UserAccount
                {
                    EmployeeID = employee.EmployeeID,
                    PasswordHash = _passwordHasher.HashPassword(employee, model.Password),
                    IsActive = true
                };

                _db.UserAccounts.Add(userAccount);
                var countUser = await _db.SaveChangesAsync();
                _logger.LogInformation("UserAccounts saved: {CountUser}, UserID: {UserId}", countUser, userAccount.UserID);

                await trx.CommitAsync();

                TempData["RegisterSuccess"] = "Registration successful. Please log in.";
                return RedirectToAction("Login_Register");
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                _logger.LogError(ex, "Registration failed while saving to DB.");
                TempData["RegisterError"] = "Registration failed: " + ex.Message;
                return View("Login_Register", model);
            }
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // repopulate departments for the dropdown
            ViewBag.Departments = _db.Departments.ToList();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "Please enter both email and password.";
                return View("Login_Register");
            }

            // Find employee by email
            var employee = _db.Employees.FirstOrDefault(e => e.Email == email);
            if (employee == null)
            {
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View("Login_Register");
            }

            // Find the matching user account
            var userAccount = _db.UserAccounts.FirstOrDefault(u => u.EmployeeID == employee.EmployeeID);
            if (userAccount == null || !userAccount.IsActive)
            {
                ViewBag.ErrorMessage = "Account not found or not active.";
                return View("Login_Register");
            }

            // Verify the password hash
            var passwordHasher = new PasswordHasher<UserAccount>();
            var result = passwordHasher.VerifyHashedPassword(userAccount, userAccount.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View("Login_Register");
            }

            // Store login info in session
            HttpContext.Session.SetInt32("EmployeeID", employee.EmployeeID);
            HttpContext.Session.SetString("EmployeeName", employee.Name);
            HttpContext.Session.SetString("Role", employee.Role);

            // Redirect based on role
            string role = employee.Role.ToLower();
            if (role == "lecturer")
                return RedirectToAction("Lecture", "Dashboard");
            else if (role == "coordinator")
                return RedirectToAction("ProgramCoordinator", "Dashboard");
            else if (role == "manager")
                return RedirectToAction("AcademicManager", "Dashboard");
            else if (role == "hr admin")
                return RedirectToAction("HRDashboard", "Dashboard");
            else
                return RedirectToAction("Index", "Dashboard");
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
