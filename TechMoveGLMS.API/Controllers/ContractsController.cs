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
    public class ContractsController : ControllerBase
    {
        private readonly IContractRepository      _repo;
        private readonly ContractValidationService _validation;
        private readonly DocumentHandlingService   _documents;

        public ContractsController(
            IContractRepository      repo,
            ContractValidationService validation,
            DocumentHandlingService   documents)
        {
            _repo       = repo;
            _validation = validation;
            _documents  = documents;
        }

        /// <summary>GET /api/contracts?startDate=&endDate=&status=</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Contract>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime?       startDate,
            [FromQuery] DateTime?       endDate,
            [FromQuery] ContractStatus? status)
        {
            var results = await _repo.FilterAsync(startDate, endDate, status);
            return Ok(results);
        }

        /// <summary>GET /api/contracts/{id}</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Contract), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var contract = await _repo.GetByIdAsync(id);
            return contract == null ? NotFound() : Ok(contract);
        }

        /// <summary>POST /api/contracts — create a new contract (with optional PDF upload)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(Contract), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create(
            [FromForm] Contract contract,
            IFormFile? pdfFile)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!_validation.IsDateRangeValid(contract.StartDate, contract.EndDate))
                return BadRequest("End date must be after start date.");

            if (pdfFile != null && pdfFile.Length > 0)
            {
                try
                {
                    contract.SignedAgreementFilePath = await _documents.UploadPdfAsync(pdfFile);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            var created = await _repo.AddAsync(contract);
            return CreatedAtAction(nameof(GetById), new { id = created.ContractId }, created);
        }

        /// <summary>PUT /api/contracts/{id} — full update</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Contract), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] Contract contract)
        {
            if (id != contract.ContractId) return BadRequest("ID mismatch.");
            if (!ModelState.IsValid)       return BadRequest(ModelState);
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var updated = await _repo.UpdateAsync(contract);
            return Ok(updated);
        }

        /// <summary>PATCH /api/contracts/{id}/status — change only the status</summary>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(typeof(Contract), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromBody] ContractStatusUpdateDto dto)
        {
            if (!Enum.TryParse<ContractStatus>(dto.Status, out var newStatus))
                return BadRequest($"Invalid status '{dto.Status}'. " +
                                  "Valid values: Draft, Active, OnHold, Expired");

            var contract = await _repo.GetByIdAsync(id);
            if (contract == null) return NotFound();

            contract.Status = newStatus;
            var updated = await _repo.UpdateAsync(contract);
            return Ok(updated);
        }

        /// <summary>DELETE /api/contracts/{id}</summary>
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
