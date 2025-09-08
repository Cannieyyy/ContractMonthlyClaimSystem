using ContractMonthlyClaimSystem.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ContractMonthlyClaimSystem.Controllers
{
    
        public class DashboardController : Controller
        {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }
        public IActionResult Lecture()
            {
                // Later: verify user role
                return View();
            }

            public IActionResult ProgramCoordinator()
            {
                // Later: verify user role
                return View();
            }

            public IActionResult AcademicManager()
            {
                // Later: verify user role
                return View();
            }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    

}
