using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS.Models;
using TechMoveGLMS.Services;

namespace TechMoveGLMS.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly IApiClientService _api;
        public ClientsController(IApiClientService api) => _api = api;

        public async Task<IActionResult> Index()
            => View(await _api.GetClientsAsync());

        public async Task<IActionResult> Details(int id)
        {
            var client = await _api.GetClientAsync(id);
            return client == null ? NotFound() : View(client);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid) return View(client);
            var result = await _api.CreateClientAsync(client);
            if (result == null) { ModelState.AddModelError("", "API error."); return View(client); }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _api.GetClientAsync(id);
            return client == null ? NotFound() : View(client);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (!ModelState.IsValid) return View(client);
            var ok = await _api.UpdateClientAsync(id, client);
            if (!ok) { ModelState.AddModelError("", "Update failed."); return View(client); }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = await _api.GetClientAsync(id);
            return client == null ? NotFound() : View(client);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _api.DeleteClientAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}