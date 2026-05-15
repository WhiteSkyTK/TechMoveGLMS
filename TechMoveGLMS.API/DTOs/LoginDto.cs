namespace TechMoveGLMS.API.DTOs
{
    // ── Auth ──────────────────────────────────────────────────────
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }

    // ── Contract ──────────────────────────────────────────────────
    public class ContractStatusUpdateDto
    {
        /// <summary>Values: Draft | Active | OnHold | Expired</summary>
        public string Status { get; set; } = string.Empty;
    }

    // ── ServiceRequest ────────────────────────────────────────────
    public class CreateServiceRequestDto
    {
        public int ContractId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal OriginalCostUSD { get; set; }
    }
}