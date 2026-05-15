using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS.API.DTOs;
using TechMoveGLMS.API.Repositories;
using TechMoveGLMS.Models;
using TechMoveGLMS.Services;

namespace TechMoveGLMS.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestRepository _repo;
        private readonly IContractRepository       _contractRepo;
        private readonly ContractValidationService _validation;

        public ServiceRequestsController(
            IServiceRequestRepository repo,
            IContractRepository       contractRepo,
            ContractValidationService validation)
        {
            _repo         = repo;
            _contractRepo = contractRepo;
            _validation   = validation;
        }

        /// <summary>GET /api/servicerequests</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ServiceRequest>), 200)]
        public async Task<IActionResult> GetAll() =>
            Ok(await _repo.GetAllAsync());

        /// <summary>GET /api/servicerequests/{id}</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ServiceRequest), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var sr = await _repo.GetByIdAsync(id);
            return sr == null ? NotFound() : Ok(sr);
        }

        /// <summary>GET /api/servicerequests/bycontract/{contractId}</summary>
        [HttpGet("bycontract/{contractId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ServiceRequest>), 200)]
        public async Task<IActionResult> GetByContract(int contractId) =>
            Ok(await _repo.GetByContractAsync(contractId));

        /// <summary>POST /api/servicerequests — workflow-validated creation with live FX</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ServiceRequest), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Create([FromBody] CreateServiceRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // ── Business rule: contract must be Active or Draft ──
            var contract = await _contractRepo.GetByIdAsync(dto.ContractId);
            if (contract == null) return NotFound("Contract not found.");

            if (!_validation.IsContractEligibleForServiceRequest(contract))
                return UnprocessableEntity(_validation.GetIneligibilityReason(contract));

            if (!_validation.IsCostValid(dto.OriginalCostUSD))
                return BadRequest("Cost must be greater than zero.");

            // ── Live FX conversion ──
            decimal zarRate;
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync("https://open.er-api.com/v6/latest/USD");
                response.EnsureSuccessStatusCode();
                var json = System.Text.Json.JsonDocument.Parse(
                    await response.Content.ReadAsStringAsync());
                zarRate = json.RootElement.GetProperty("rates").GetProperty("ZAR").GetDecimal();
            }
            catch
            {
                return StatusCode(503, "Currency API unavailable. Please try again later.");
            }

            var sr = new ServiceRequest
            {
                ContractId       = dto.ContractId,
                Description      = dto.Description,
                OriginalCostUSD  = dto.OriginalCostUSD,
                ConvertedCostZAR = _validation.ConvertUsdToZar(dto.OriginalCostUSD, zarRate),
                Status           = "Pending"
            };

            var created = await _repo.AddAsync(sr);
            return CreatedAtAction(nameof(GetById), new { id = created.RequestId }, created);
        }

        /// <summary>PUT /api/servicerequests/{id}</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ServiceRequest), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceRequest sr)
        {
            if (id != sr.RequestId) return BadRequest("ID mismatch.");
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            return Ok(await _repo.UpdateAsync(sr));
        }

        /// <summary>DELETE /api/servicerequests/{id}</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
