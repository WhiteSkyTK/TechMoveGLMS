using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // <--- ADD THIS
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

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create()
        {
            // FIX: Use ViewBag.ContractId and wrap your items inside a SelectList
            var contracts = await _api.GetContractsAsync();
            ViewBag.ContractId = new SelectList(contracts, "ContractId", "ContractId");
            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest sr)
        {
            var (result, error) = await _api.CreateServiceRequestAsync(
                sr.ContractId, sr.Description, sr.OriginalCostUSD);

            if (result == null)
            {
                ModelState.AddModelError("", error ?? "API error creating request.");
                // FIX: Repopulate using the correct SelectList name on validation failure
                var contracts = await _api.GetContractsAsync();
                ViewBag.ContractId = new SelectList(contracts, "ContractId", "ContractId");
                return View(sr);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var sr = await _api.GetServiceRequestAsync(id);
            if (sr == null) return NotFound();

            // FIX: Set select list name and pre-select current item
            var contracts = await _api.GetContractsAsync();
            ViewBag.ContractId = new SelectList(contracts, "ContractId", "ContractId", sr.ContractId);
            return View(sr);
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequest sr)
        {
            if (!ModelState.IsValid)
            {
                var contracts = await _api.GetContractsAsync();
                ViewBag.ContractId = new SelectList(contracts, "ContractId", "ContractId", sr.ContractId);
                return View(sr);
            }
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