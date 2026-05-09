using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Models;
using TechMoveGLMS.Services;

namespace TechMoveGLMS.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ContractValidationService _validationService;

        public ServiceRequestsController(
            ApplicationDbContext context,
            ContractValidationService validationService)
        {
            _context = context;
            _validationService = validationService;
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            var requests = _context.ServiceRequests.Include(s => s.Contract);
            return View(await requests.ToListAsync());
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (serviceRequest == null) return NotFound();
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Create
        public IActionResult Create()
        {
            ViewData["ContractId"] = new SelectList(_context.Contracts, "ContractId", "ContractId");
            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("RequestId,ContractId,Description,OriginalCostUSD,Status")] ServiceRequest serviceRequest)
        {
            if (ModelState.IsValid)
            {
                // ── STEP 1: Workflow validation via ContractValidationService ──
                var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);
                if (contract == null) return NotFound();

                if (!_validationService.IsContractEligibleForServiceRequest(contract))
                {
                    ModelState.AddModelError("",
                        _validationService.GetIneligibilityReason(contract));
                    ViewData["ContractId"] = new SelectList(
                        _context.Contracts, "ContractId", "ContractId",
                        serviceRequest.ContractId);
                    return View(serviceRequest);
                }

                // ── STEP 2: Cost validation ──
                if (!_validationService.IsCostValid(serviceRequest.OriginalCostUSD))
                {
                    ModelState.AddModelError("OriginalCostUSD",
                        "Cost must be greater than zero.");
                    ViewData["ContractId"] = new SelectList(
                        _context.Contracts, "ContractId", "ContractId",
                        serviceRequest.ContractId);
                    return View(serviceRequest);
                }

                // ── STEP 3: Live currency conversion via ExchangeRate API ──
                try
                {
                    using var client = new HttpClient();
                    var response = await client.GetAsync("https://open.er-api.com/v6/latest/USD");
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var json = System.Text.Json.JsonDocument.Parse(content);
                    var zarRate = json.RootElement
                        .GetProperty("rates")
                        .GetProperty("ZAR")
                        .GetDecimal();

                    serviceRequest.ConvertedCostZAR =
                        _validationService.ConvertUsdToZar(serviceRequest.OriginalCostUSD, zarRate);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("",
                        "Currency API is currently unavailable. Please try again later.");
                    ViewData["ContractId"] = new SelectList(
                        _context.Contracts, "ContractId", "ContractId",
                        serviceRequest.ContractId);
                    return View(serviceRequest);
                }

                // ── STEP 4: Save ──
                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ContractId"] = new SelectList(
                _context.Contracts, "ContractId", "ContractId",
                serviceRequest.ContractId);
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null) return NotFound();

            ViewData["ContractId"] = new SelectList(
                _context.Contracts, "ContractId", "ContractId",
                serviceRequest.ContractId);
            return View(serviceRequest);
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("RequestId,ContractId,Description,OriginalCostUSD,ConvertedCostZAR,Status")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.RequestId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serviceRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(serviceRequest.RequestId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ContractId"] = new SelectList(
                _context.Contracts, "ContractId", "ContractId",
                serviceRequest.ContractId);
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (serviceRequest == null) return NotFound();
            return View(serviceRequest);
        }

        // POST: ServiceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest != null)
                _context.ServiceRequests.Remove(serviceRequest);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceRequestExists(int id) =>
            _context.ServiceRequests.Any(e => e.RequestId == id);
    }
}