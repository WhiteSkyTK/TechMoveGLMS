using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS.Models;
using TechMoveGLMS.Services;

namespace TechMoveGLMS.Controllers
{
    [Authorize]
    public class ContractsController : Controller
    {
        private readonly IApiClientService _api;
        public ContractsController(IApiClientService api) => _api = api;

        public async Task<IActionResult> Index(
            DateTime? startDate, DateTime? endDate, ContractStatus? searchStatus)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.SearchStatus = searchStatus;
            return View(await _api.GetContractsAsync(startDate, endDate, searchStatus));
        }

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _api.GetContractAsync(id);
            return contract == null ? NotFound() : View(contract);
        }

        public async Task<IActionResult> Create()
        {
            // Populate client dropdown from API
            ViewBag.Clients = await _api.GetClientsAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract, IFormFile? pdfFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Clients = await _api.GetClientsAsync();
                return View(contract);
            }

            Stream? stream = null;
            if (pdfFile != null && pdfFile.Length > 0)
                stream = pdfFile.OpenReadStream();

            var result = await _api.CreateContractAsync(contract, stream, pdfFile?.FileName);
            if (result == null)
            {
                ModelState.AddModelError("", "Failed to create contract via API.");
                ViewBag.Clients = await _api.GetClientsAsync();
                return View(contract);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _api.GetContractAsync(id);
            if (contract == null) return NotFound();
            ViewBag.Clients = await _api.GetClientsAsync();
            return View(contract);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Clients = await _api.GetClientsAsync();
                return View(contract);
            }
            await _api.UpdateContractAsync(id, contract);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _api.GetContractAsync(id);
            return contract == null ? NotFound() : View(contract);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _api.DeleteContractAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}