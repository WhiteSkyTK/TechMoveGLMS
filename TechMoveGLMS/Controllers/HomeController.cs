using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TechMoveGLMS.Models;
using TechMoveGLMS.Services;

namespace TechMoveGLMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiClientService _api;

        public HomeController(IApiClientService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Use the existing service methods to populate dashboard cards
                var contracts = await _api.GetContractsAsync();
                var requests = await _api.GetServiceRequestsAsync();

                ViewBag.ActiveContracts = contracts.Count(c => c.Status == ContractStatus.Active);
                ViewBag.PendingRequests = requests.Count(r => r.Status == "Pending");
                ViewBag.TotalPendingValue = requests
                    .Where(r => r.Status == "Pending")
                    .Sum(r => r.ConvertedCostZAR);
            }
            catch
            {
                // If API is unreachable, show zeros rather than crash
                ViewBag.ActiveContracts = 0;
                ViewBag.PendingRequests = 0;
                ViewBag.TotalPendingValue = 0m;
            }

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
    }
}