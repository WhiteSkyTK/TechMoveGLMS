using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TechMoveGLMS.Models;

namespace TechMoveGLMS.Services
{
    /// <summary>
    /// Concrete implementation of IApiClientService.
    /// All MVC controllers go through this class — zero direct DB access.
    /// Reads the JWT token from the HTTP cookie and attaches it to every request.
    /// </summary>
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient          _http;
        private readonly IHttpContextAccessor _ctx;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        public ApiClientService(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx  = ctx;
        }

        // ── Attach JWT from cookie to each outbound request ───────
        private void AttachToken()
        {
            var token = _ctx.HttpContext?.Request.Cookies["glms_token"];
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        private static StringContent Json<T>(T obj) =>
            new(JsonSerializer.Serialize(obj, _json),
                Encoding.UTF8, "application/json");

        private static async Task<T?> Read<T>(HttpResponseMessage resp)
        {
            if (!resp.IsSuccessStatusCode) return default;
            var body = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(body, _json);
        }

        // ══════════════════════════════════════════════════════════
        // CLIENTS
        // ══════════════════════════════════════════════════════════

        public async Task<List<Client>> GetClientsAsync()
        {
            AttachToken();
            var resp = await _http.GetAsync("api/clients");
            return await Read<List<Client>>(resp) ?? new();
        }

        public async Task<Client?> GetClientAsync(int id)
        {
            AttachToken();
            var resp = await _http.GetAsync($"api/clients/{id}");
            return await Read<Client>(resp);
        }

        public async Task<Client?> CreateClientAsync(Client client)
        {
            AttachToken();
            var resp = await _http.PostAsync("api/clients", Json(client));
            return await Read<Client>(resp);
        }

        public async Task<bool> UpdateClientAsync(int id, Client client)
        {
            AttachToken();
            var resp = await _http.PutAsync($"api/clients/{id}", Json(client));
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            AttachToken();
            var resp = await _http.DeleteAsync($"api/clients/{id}");
            return resp.IsSuccessStatusCode;
        }

        // ══════════════════════════════════════════════════════════
        // CONTRACTS
        // ══════════════════════════════════════════════════════════

        public async Task<List<Contract>> GetContractsAsync(
            DateTime? startDate = null, DateTime? endDate = null, ContractStatus? status = null)
        {
            AttachToken();
            var qs = new List<string>();
            if (startDate.HasValue) qs.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue)   qs.Add($"endDate={endDate:yyyy-MM-dd}");
            if (status.HasValue)    qs.Add($"status={(int)status}");

            var url  = "api/contracts" + (qs.Any() ? "?" + string.Join("&", qs) : "");
            var resp = await _http.GetAsync(url);
            return await Read<List<Contract>>(resp) ?? new();
        }

        public async Task<Contract?> GetContractAsync(int id)
        {
            AttachToken();
            var resp = await _http.GetAsync($"api/contracts/{id}");
            return await Read<Contract>(resp);
        }

        public async Task<Contract?> CreateContractAsync(
            Contract contract, Stream? pdfStream, string? pdfFileName)
        {
            AttachToken();
            using var form = new MultipartFormDataContent();

            // Add contract fields
            form.Add(new StringContent(contract.ClientId.ToString()),       "ClientId");
            form.Add(new StringContent(contract.StartDate.ToString("o")),   "StartDate");
            form.Add(new StringContent(contract.EndDate.ToString("o")),     "EndDate");
            form.Add(new StringContent(((int)contract.Status).ToString()),  "Status");
            form.Add(new StringContent(((int)contract.Level).ToString()),   "Level");

            // Add PDF if provided
            if (pdfStream != null && !string.IsNullOrEmpty(pdfFileName))
                form.Add(new StreamContent(pdfStream), "pdfFile", pdfFileName);

            var resp = await _http.PostAsync("api/contracts", form);
            return await Read<Contract>(resp);
        }

        public async Task<bool> UpdateContractAsync(int id, Contract contract)
        {
            AttachToken();
            var resp = await _http.PutAsync($"api/contracts/{id}", Json(contract));
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> PatchContractStatusAsync(int id, string newStatus)
        {
            AttachToken();
            var resp = await _http.PatchAsync(
                $"api/contracts/{id}/status",
                Json(new { Status = newStatus }));
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteContractAsync(int id)
        {
            AttachToken();
            var resp = await _http.DeleteAsync($"api/contracts/{id}");
            return resp.IsSuccessStatusCode;
        }

        // ══════════════════════════════════════════════════════════
        // SERVICE REQUESTS
        // ══════════════════════════════════════════════════════════

        public async Task<List<ServiceRequest>> GetServiceRequestsAsync()
        {
            AttachToken();
            var resp = await _http.GetAsync("api/servicerequests");
            return await Read<List<ServiceRequest>>(resp) ?? new();
        }

        public async Task<ServiceRequest?> GetServiceRequestAsync(int id)
        {
            AttachToken();
            var resp = await _http.GetAsync($"api/servicerequests/{id}");
            return await Read<ServiceRequest>(resp);
        }

        public async Task<(ServiceRequest? Result, string? Error)> CreateServiceRequestAsync(
            int contractId, string description, decimal usdAmount)
        {
            AttachToken();
            var payload = new { ContractId = contractId, Description = description, OriginalCostUSD = usdAmount };
            var resp = await _http.PostAsync("api/servicerequests", Json(payload));

            if (resp.IsSuccessStatusCode)
                return (await Read<ServiceRequest>(resp), null);

            var errorBody = await resp.Content.ReadAsStringAsync();
            return (null, errorBody);
        }

        public async Task<bool> UpdateServiceRequestAsync(int id, ServiceRequest sr)
        {
            AttachToken();
            var resp = await _http.PutAsync($"api/servicerequests/{id}", Json(sr));
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteServiceRequestAsync(int id)
        {
            AttachToken();
            var resp = await _http.DeleteAsync($"api/servicerequests/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}
