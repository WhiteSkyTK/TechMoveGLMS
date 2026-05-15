using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS.Models;
using TechMoveGLMS.Services;

namespace TechMoveGLMS.Controllers
{
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly IApiClientService _api;
        public ServiceRequestsController(IApiClientService api) => _api = api;

        public async Task<IActionResult> Index()
            => View(await _api.GetServiceRequestsAsync());

        public async Task<IActionResult> Details(int id)
        {
            var sr = await _api.GetServiceRequestAsync(id);
            return sr == null ? NotFound() : View(sr);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Contracts = await _api.GetContractsAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest sr)
        {
            var (result, error) = await _api.CreateServiceRequestAsync(
                sr.ContractId, sr.Description, sr.OriginalCostUSD);

            if (result == null)
            {
                ModelState.AddModelError("", error ?? "API error creating request.");
                ViewBag.Contracts = await _api.GetContractsAsync();
                return View(sr);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var sr = await _api.GetServiceRequestAsync(id);
            if (sr == null) return NotFound();
            ViewBag.Contracts = await _api.GetContractsAsync();
            return View(sr);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequest sr)
        {
            await _api.UpdateServiceRequestAsync(id, sr);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var sr = await _api.GetServiceRequestAsync(id);
            return sr == null ? NotFound() : View(sr);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _api.DeleteServiceRequestAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}