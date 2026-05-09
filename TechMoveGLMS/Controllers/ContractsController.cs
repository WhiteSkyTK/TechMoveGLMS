using System;
using System.Collections.Generic;
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
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DocumentHandlingService _documentService;

        public ContractsController(ApplicationDbContext context, DocumentHandlingService documentService)
        {
            _context = context;
            _documentService = documentService;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, ContractStatus? searchStatus)
        {
            // Start with the base query including the Client relationship
            var contractsQuery = _context.Contracts.Include(c => c.Client).AsQueryable();

            // LINQ Filter: Date Range
            if (startDate.HasValue)
                contractsQuery = contractsQuery.Where(c => c.StartDate >= startDate.Value);

            if (endDate.HasValue)
                contractsQuery = contractsQuery.Where(c => c.EndDate <= endDate.Value);

            // LINQ Filter: Status
            if (searchStatus.HasValue)
                contractsQuery = contractsQuery.Where(c => c.Status == searchStatus.Value);

            // Pass filter values back to keep the form populated
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.SearchStatus = searchStatus;

            return View(await contractsQuery.ToListAsync());
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.ContractId == id);

            if (contract == null) return NotFound();

            return View(contract);
        }

        // GET: Contracts/Create
        public IActionResult Create()
        {
            // FIX: Display the client's Name (not ContactEmail) in the dropdown
            ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name");
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ContractId,ClientId,StartDate,EndDate,Status,Level")] Contract contract,
            IFormFile? pdfFile)
        {
            if (ModelState.IsValid)
            {
                // FIX: Handle the PDF file upload using DocumentHandlingService
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    try
                    {
                        contract.SignedAgreementFilePath = await _documentService.UploadPdfAsync(pdfFile);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Non-PDF file was uploaded — surface the error to the user
                        ModelState.AddModelError("", ex.Message);
                        ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name", contract.ClientId);
                        return View(contract);
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", "File upload failed. Please try again.");
                        ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name", contract.ClientId);
                        return View(contract);
                    }
                }

                _context.Add(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name", contract.ClientId);
            return View(contract);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name", contract.ClientId);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ContractId,ClientId,StartDate,EndDate,Status,Level,SignedAgreementFilePath")] Contract contract)
        {
            if (id != contract.ContractId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.ContractId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name", contract.ClientId);
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.ContractId == id);

            if (contract == null) return NotFound();

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.ContractId == id);
        }
    }
}