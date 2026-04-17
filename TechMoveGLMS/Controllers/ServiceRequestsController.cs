using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Models;

namespace TechMoveGLMS.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ServiceRequests1
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ServiceRequests.Include(s => s.Contract);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ServiceRequests1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // GET: ServiceRequests1/Create
        public IActionResult Create()
        {
            ViewData["ContractId"] = new SelectList(_context.Contracts, "ContractId", "ContractId");
            return View();
        }

        // POST: ServiceRequests1/Create.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestId,ContractId,Description,OriginalCostUSD,Status")] ServiceRequest serviceRequest)
        {
            if (ModelState.IsValid)
            {
                // 1. WORKFLOW VALIDATION: Check Contract Status
                var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);
                if (contract == null)
                {
                    return NotFound();
                }

                if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
                {
                    // Block the submission and return an error to the UI
                    ModelState.AddModelError("", "Action Denied: Cannot raise a service request against an Expired or On Hold contract.");
                    ViewData["ContractId"] = new SelectList(_context.Contracts, "ContractId", "ContractId", serviceRequest.ContractId);
                    return View(serviceRequest);
                }

                // 2. EXTERNAL API INTEGRATION: Calculate ZAR securely on the server
                try
                {
                    // Note: If you injected your ICurrencyStrategy, use that here. 
                    // For now, using direct HttpClient as requested by the rubric.
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync("https://open.er-api.com/v6/latest/USD");
                        response.EnsureSuccessStatusCode();

                        var content = await response.Content.ReadAsStringAsync();
                        var jsonDocument = System.Text.Json.JsonDocument.Parse(content);
                        var zarRate = jsonDocument.RootElement.GetProperty("rates").GetProperty("ZAR").GetDecimal();

                        // Apply the math
                        serviceRequest.ConvertedCostZAR = Math.Round(serviceRequest.OriginalCostUSD * zarRate, 2);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Currency API is currently unavailable. Please try again later.");
                    ViewData["ContractId"] = new SelectList(_context.Contracts, "ContractId", "ContractId", serviceRequest.ContractId);
                    return View(serviceRequest);
                }

                // 3. Save to Database
                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ContractId"] = new SelectList(_context.Contracts, "ContractId", "ContractId", serviceRequest.ContractId);
            return View(serviceRequest);
        }

        // GET: ServiceRequests1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }
            ViewData["ContractId"] = new SelectList(_context.Contracts, "ContractId", "ContractId", serviceRequest.ContractId);
            return View(serviceRequest);
        }

        // POST: ServiceRequests1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestId,ContractId,Description,OriginalCostUSD,ConvertedCostZAR,Status")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.RequestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serviceRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(serviceRequest.RequestId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ContractId"] = new SelectList(_context.Contracts, "ContractId", "ContractId", serviceRequest.ContractId);
            return View(serviceRequest);
        }

        // GET: ServiceRequests1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // POST: ServiceRequests1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest != null)
            {
                _context.ServiceRequests.Remove(serviceRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests.Any(e => e.RequestId == id);
        }
    }
}
