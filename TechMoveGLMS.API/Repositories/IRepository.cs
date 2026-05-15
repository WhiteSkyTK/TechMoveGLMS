using TechMoveGLMS.Models;

namespace TechMoveGLMS.API.Repositories
{
    // ── Generic base ──────────────────────────────────────────────
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }

    // ── Client ────────────────────────────────────────────────────
    public interface IClientRepository : IRepository<Client> { }

    // ── Contract ──────────────────────────────────────────────────
    public interface IContractRepository : IRepository<Contract>
    {
        Task<IEnumerable<Contract>> FilterAsync(
            DateTime? startDate, DateTime? endDate, ContractStatus? status);
    }

    // ── ServiceRequest ────────────────────────────────────────────
    public interface IServiceRequestRepository : IRepository<ServiceRequest>
    {
        Task<IEnumerable<ServiceRequest>> GetByContractAsync(int contractId);
    }
}