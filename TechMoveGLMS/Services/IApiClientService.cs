using TechMoveGLMS.Models;

namespace TechMoveGLMS.Services
{
    /// <summary>
    /// Abstraction over all HTTP calls to TechMoveGLMS.API.
    /// MVC controllers depend on this interface — not on DbContext.
    /// This is the Service Layer that decouples the Presentation Layer from data.
    /// </summary>
    public interface IApiClientService
    {
        // ── Clients ───────────────────────────────────────────────
        Task<List<Client>>    GetClientsAsync();
        Task<Client?>         GetClientAsync(int id);
        Task<Client?>         CreateClientAsync(Client client);
        Task<bool>            UpdateClientAsync(int id, Client client);
        Task<bool>            DeleteClientAsync(int id);

        // ── Contracts ─────────────────────────────────────────────
        Task<List<Contract>>  GetContractsAsync(
            DateTime? startDate = null,
            DateTime? endDate   = null,
            ContractStatus? status = null);
        Task<Contract?>       GetContractAsync(int id);
        Task<Contract?>       CreateContractAsync(Contract contract, Stream? pdfStream, string? pdfFileName);
        Task<bool>            UpdateContractAsync(int id, Contract contract);
        Task<bool>            PatchContractStatusAsync(int id, string newStatus);
        Task<bool>            DeleteContractAsync(int id);

        // ── Service Requests ──────────────────────────────────────
        Task<List<ServiceRequest>> GetServiceRequestsAsync();
        Task<ServiceRequest?>      GetServiceRequestAsync(int id);
        Task<(ServiceRequest? Result, string? Error)> CreateServiceRequestAsync(
            int contractId, string description, decimal usdAmount);
        Task<bool>                 UpdateServiceRequestAsync(int id, ServiceRequest sr);
        Task<bool>                 DeleteServiceRequestAsync(int id);
    }
}
