using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS.Models;
using TechMoveGLMS.Data;   
using System.Threading.Tasks;

namespace TechMoveGLMS.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection for the database
        public ServiceRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ServiceRequests/Create
        public IActionResult Create()
        {
            // Usually, you pass a list of Contracts here for a dropdown menu
            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest model)
        {
            if (ModelState.IsValid)
            {
                // 1. WORKFLOW VALIDATION LOGIC GOES HERE
                // Find the contract linked to this new service request
                var contract = await _context.Contracts.FindAsync(model.ContractId);

                // Check if the contract exists AND if its status is invalid
                if (contract != null &&
                   (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold))
                {
                    // This adds the error message to the page so the user sees it
                    ModelState.AddModelError("", "Cannot create a service request for an Expired or On Hold contract.");

                    // Return the view so they can fix their mistake
                    return View(model);
                }

                // 2. IF VALIDATION PASSES, SAVE TO DATABASE
                _context.Add(model);
                await _context.SaveChangesAsync();

                // Redirect back to the list of requests
                return RedirectToAction(nameof(Index));
            }

            // If the model itself is invalid (e.g., missing a required field), return the view
            return View(model);
        }
    }
}