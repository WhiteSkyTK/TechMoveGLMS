using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TechMoveGLMS.Data;
using TechMoveGLMS.Models;
using System.Threading.Tasks;
using System.Linq;

namespace TechMoveGLMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Inject the Database Context
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Calculate Active Contracts
            // We use .CountAsync() to quickly get the number without loading all records into memory
            ViewBag.ActiveContracts = await _context.Contracts
                .CountAsync(c => c.Status == TechMoveGLMS.Models.ContractStatus.Active);

            // 2. Calculate Pending Service Requests
            ViewBag.PendingRequests = await _context.ServiceRequests
                .CountAsync(r => r.Status == "Pending");

            // 3. Calculate Total Value of Pending Requests in ZAR
            // This shows off your LINQ math skills to the lecturer!
            var totalValue = await _context.ServiceRequests
                .Where(r => r.Status == "Pending")
                .SumAsync(r => r.ConvertedCostZAR);

            ViewBag.TotalPendingValue = totalValue;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
