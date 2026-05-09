using TechMoveGLMS.Models;

namespace TechMoveGLMS.Services
{
    /// <summary>
    /// Encapsulates all business rule validation for Contracts and Service Requests.
    /// Separating this logic from Controllers makes it independently testable (TDD principle).
    /// This is the Strategy pattern in action — the controller delegates validation decisions here.
    /// </summary>
    public class ContractValidationService
    {
        // ============================================================
        // 1. SERVICE REQUEST WORKFLOW RULES
        // ============================================================

        /// <summary>
        /// Core business rule: A service request can only be raised
        /// against a contract that is Active or Draft.
        /// Expired and OnHold contracts must be blocked.
        /// </summary>
        public bool IsContractEligibleForServiceRequest(Contract contract)
        {
            if (contract == null) return false;

            return contract.Status == ContractStatus.Active ||
                   contract.Status == ContractStatus.Draft;
        }

        /// <summary>
        /// Returns the human-readable reason why a contract blocks a request.
        /// Used to populate the ModelState error message in the controller.
        /// </summary>
        public string GetIneligibilityReason(Contract contract)
        {
            if (contract == null)
                return "Contract not found.";

            return contract.Status switch
            {
                ContractStatus.Expired =>
                    "Action Denied: Cannot raise a service request against an Expired contract.",
                ContractStatus.OnHold =>
                    "Action Denied: Cannot raise a service request against a contract that is On Hold.",
                _ => string.Empty
            };
        }

        // ============================================================
        // 2. CONTRACT DATE RULES
        // ============================================================

        /// <summary>
        /// The end date of a contract must be strictly after the start date.
        /// </summary>
        public bool IsDateRangeValid(DateTime startDate, DateTime endDate)
        {
            return endDate > startDate;
        }

        /// <summary>
        /// A contract start date should not be in the past by more than a
        /// reasonable grace period (e.g. back-dating is limited to 1 year).
        /// </summary>
        public bool IsStartDateReasonable(DateTime startDate)
        {
            return startDate >= DateTime.Today.AddYears(-1);
        }

        // ============================================================
        // 3. COST VALIDATION RULES
        // ============================================================

        /// <summary>
        /// The USD cost of a service request must be a positive, non-zero value.
        /// </summary>
        public bool IsCostValid(decimal usdAmount)
        {
            return usdAmount > 0;
        }

        /// <summary>
        /// Performs the currency conversion using a supplied exchange rate.
        /// Centralised here so the math is tested in one place.
        /// </summary>
        public decimal ConvertUsdToZar(decimal usdAmount, decimal zarRate)
        {
            if (usdAmount < 0)
                throw new ArgumentOutOfRangeException(nameof(usdAmount), "USD amount cannot be negative.");

            if (zarRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(zarRate), "Exchange rate must be a positive value.");

            return Math.Round(usdAmount * zarRate, 2);
        }
    }
}