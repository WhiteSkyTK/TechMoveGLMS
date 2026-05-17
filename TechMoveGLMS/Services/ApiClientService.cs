using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TechMoveGLMS.Models;

namespace TechMoveGLMS.Services
{
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        public ApiClientService(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        // ── CORE PIPELINE: Safely attach token per request ────────
        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string uri, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, uri);
            if (content != null) request.Content = content;

            // Extract JWT from cookie
            var token = _ctx.HttpContext?.Request.Cookies["glms_token"];
            if (!string.IsNullOrEmpty(token))
            {
                // Attach securely to this specific request, NOT the global client
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _http.SendAsync(request);
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
            var resp = await SendAsync(HttpMethod.Get, "api/clients");
            return await Read<List<Client>>(resp) ?? new();
        }

        public async Task<Client?> GetClientAsync(int id)
        {
            var resp = await SendAsync(HttpMethod.Get, $"api/clients/{id}");
            return await Read<Client>(resp);
        }

        public async Task<Client?> CreateClientAsync(Client client)
        {
            var resp = await SendAsync(HttpMethod.Post, "api/clients", Json(client));
            return await Read<Client>(resp);
        }

        public async Task<bool> UpdateClientAsync(int id, Client client)
        {
            var resp = await SendAsync(HttpMethod.Put, $"api/clients/{id}", Json(client));
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            var resp = await SendAsync(HttpMethod.Delete, $"api/clients/{id}");
            return resp.IsSuccessStatusCode;
        }

        // ══════════════════════════════════════════════════════════
        // CONTRACTS
        // ══════════════════════════════════════════════════════════

        public async Task<List<Contract>> GetContractsAsync(
            DateTime? startDate = null, DateTime? endDate = null, ContractStatus? status = null)
        {
            var qs = new List<string>();
            if (startDate.HasValue) qs.Add($"startDate={startDate:yyyy-MM-dd}");
            if (endDate.HasValue) qs.Add($"endDate={endDate:yyyy-MM-dd}");
            if (status.HasValue) qs.Add($"status={(int)status}");

            var url = "api/contracts" + (qs.Any() ? "?" + string.Join("&", qs) : "");
            var resp = await SendAsync(HttpMethod.Get, url);
            return await Read<List<Contract>>(resp) ?? new();
        }

        public async Task<Contract?> GetContractAsync(int id)
        {
            var resp = await SendAsync(HttpMethod.Get, $"api/contracts/{id}");
            return await Read<Contract>(resp);
        }

        public async Task<Contract?> CreateContractAsync(
            Contract contract, Stream? pdfStream, string? pdfFileName)
        {
            using var form = new MultipartFormDataContent();

            // Add contract fields
            form.Add(new StringContent(contract.ClientId.ToString()), "ClientId");
            form.Add(new StringContent(contract.StartDate.ToString("o")), "StartDate");
            form.Add(new StringContent(contract.EndDate.ToString("o")), "EndDate");
            form.Add(new StringContent(((int)contract.Status).ToString()), "Status");
            form.Add(new StringContent(((int)contract.Level).ToString()), "Level");

            // Add PDF if provided
            if (pdfStream != null && !string.IsNullOrEmpty(pdfFileName))
                form.Add(new StreamContent(pdfStream), "pdfFile", pdfFileName);

            var resp = await SendAsync(HttpMethod.Post, "api/contracts", form);
            return await Read<Contract>(resp);
        }

        public async Task<bool> UpdateContractAsync(int id, Contract contract)
        {
            var resp = await SendAsync(HttpMethod.Put, $"api/contracts/{id}", Json(contract));
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> PatchContractStatusAsync(int id, string newStatus)
        {
            var resp = await SendAsync(HttpMethod.Patch, $"api/contracts/{id}/status", Json(new { Status = newStatus }));
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteContractAsync(int id)
        {
            var resp = await SendAsync(HttpMethod.Delete, $"api/contracts/{id}");
            return resp.IsSuccessStatusCode;
        }

        // ══════════════════════════════════════════════════════════
        // SERVICE REQUESTS
        // ══════════════════════════════════════════════════════════

        public async Task<List<ServiceRequest>> GetServiceRequestsAsync()
        {
            var resp = await SendAsync(HttpMethod.Get, "api/servicerequests");
            return await Read<List<ServiceRequest>>(resp) ?? new();
        }

        public async Task<ServiceRequest?> GetServiceRequestAsync(int id)
        {
            var resp = await SendAsync(HttpMethod.Get, $"api/servicerequests/{id}");
            return await Read<ServiceRequest>(resp);
        }

        public async Task<(ServiceRequest? Result, string? Error)> CreateServiceRequestAsync(
            int contractId, string description, decimal usdAmount)
        {
            var payload = new { ContractId = contractId, Description = description, OriginalCostUSD = usdAmount };
            var resp = await SendAsync(HttpMethod.Post, "api/servicerequests", Json(payload));

            if (resp.IsSuccessStatusCode)
                return (await Read<ServiceRequest>(resp), null);

            var errorBody = await resp.Content.ReadAsStringAsync();
            return (null, errorBody);
        }

        public async Task<bool> UpdateServiceRequestAsync(int id, ServiceRequest sr)
        {
            var resp = await SendAsync(HttpMethod.Put, $"api/servicerequests/{id}", Json(sr));
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteServiceRequestAsync(int id)
        {
            var resp = await SendAsync(HttpMethod.Delete, $"api/servicerequests/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}