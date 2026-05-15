using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS.API.Repositories;
using TechMoveGLMS.Models;

namespace TechMoveGLMS.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientRepository _repo;
        public ClientsController(IClientRepository repo) => _repo = repo;

        /// <summary>GET /api/clients — list all clients</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Client>), 200)]
        public async Task<IActionResult> GetAll() =>
            Ok(await _repo.GetAllAsync());

        /// <summary>GET /api/clients/{id}</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Client), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var client = await _repo.GetByIdAsync(id);
            return client == null ? NotFound() : Ok(client);
        }

        /// <summary>POST /api/clients — create a new client</summary>
        [HttpPost]
        [ProducesResponseType(typeof(Client), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] Client client)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _repo.AddAsync(client);
            return CreatedAtAction(nameof(GetById),
                new { id = created.ClientId }, created);
        }

        /// <summary>PUT /api/clients/{id} — update a client</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Client), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] Client client)
        {
            if (id != client.ClientId) return BadRequest("ID mismatch.");
            if (!ModelState.IsValid)   return BadRequest(ModelState);
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var updated = await _repo.UpdateAsync(client);
            return Ok(updated);
        }

        /// <summary>DELETE /api/clients/{id}</summary>
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
