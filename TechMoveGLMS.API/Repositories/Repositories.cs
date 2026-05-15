using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Models;

namespace TechMoveGLMS.API.Repositories
{
    // ── Client ────────────────────────────────────────────────────
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _db;
        public ClientRepository(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<Client>> GetAllAsync() =>
            await _db.Clients.ToListAsync();

        public async Task<Client?> GetByIdAsync(int id) =>
            await _db.Clients.FindAsync(id);

        public async Task<Client> AddAsync(Client entity)
        {
            _db.Clients.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<Client> UpdateAsync(Client entity)
        {
            _db.Clients.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Clients.FindAsync(id);
            if (entity != null) { _db.Clients.Remove(entity); await _db.SaveChangesAsync(); }
        }
    }

    // ── Contract ──────────────────────────────────────────────────
    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext _db;
        public ContractRepository(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<Contract>> GetAllAsync() =>
            await _db.Contracts.Include(c => c.Client).ToListAsync();

        public async Task<Contract?> GetByIdAsync(int id) =>
            await _db.Contracts.Include(c => c.Client)
                               .FirstOrDefaultAsync(c => c.ContractId == id);

        public async Task<IEnumerable<Contract>> FilterAsync(
            DateTime? startDate, DateTime? endDate, ContractStatus? status)
        {
            var query = _db.Contracts.Include(c => c.Client).AsQueryable();

            if (startDate.HasValue) query = query.Where(c => c.StartDate >= startDate.Value);
            if (endDate.HasValue)   query = query.Where(c => c.EndDate   <= endDate.Value);
            if (status.HasValue)    query = query.Where(c => c.Status    == status.Value);

            return await query.ToListAsync();
        }

        public async Task<Contract> AddAsync(Contract entity)
        {
            _db.Contracts.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<Contract> UpdateAsync(Contract entity)
        {
            _db.Contracts.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.Contracts.FindAsync(id);
            if (entity != null) { _db.Contracts.Remove(entity); await _db.SaveChangesAsync(); }
        }
    }

    // ── ServiceRequest ────────────────────────────────────────────
    public class ServiceRequestRepository : IServiceRequestRepository
    {
        private readonly ApplicationDbContext _db;
        public ServiceRequestRepository(ApplicationDbContext db) => _db = db;

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync() =>
            await _db.ServiceRequests.Include(s => s.Contract).ToListAsync();

        public async Task<ServiceRequest?> GetByIdAsync(int id) =>
            await _db.ServiceRequests.Include(s => s.Contract)
                                     .FirstOrDefaultAsync(s => s.RequestId == id);

        public async Task<IEnumerable<ServiceRequest>> GetByContractAsync(int contractId) =>
            await _db.ServiceRequests.Where(s => s.ContractId == contractId).ToListAsync();

        public async Task<ServiceRequest> AddAsync(ServiceRequest entity)
        {
            _db.ServiceRequests.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<ServiceRequest> UpdateAsync(ServiceRequest entity)
        {
            _db.ServiceRequests.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _db.ServiceRequests.FindAsync(id);
            if (entity != null) { _db.ServiceRequests.Remove(entity); await _db.SaveChangesAsync(); }
        }
    }
}
