using TechMoveGLMS.Models;
using TechMoveGLMS.Services;

namespace Tests
{
    /// <summary>
    /// Tests the core business workflow rules managed by ContractValidationService.
    /// This is the most important test class for the rubric — it proves the
    /// "Expired/OnHold contract blocks requests" rule works independently of the UI.
    /// </summary>
    public class WorkflowValidationTests
    {
        private readonly ContractValidationService _service = new();

        // ════════════════════════════════════════════════════════════
        // CONTRACT ELIGIBILITY FOR SERVICE REQUESTS
        // ════════════════════════════════════════════════════════════

        [Fact]
        public void IsContractEligible_ActiveContract_ReturnsTrue()
        {
            // Arrange
            var contract = new Contract
            {
                ContractId = 1,
                Status = ContractStatus.Active,
                StartDate = DateTime.Today.AddMonths(-1),
                EndDate = DateTime.Today.AddYears(1)
            };

            // Act
            bool result = _service.IsContractEligibleForServiceRequest(contract);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsContractEligible_DraftContract_ReturnsTrue()
        {
            // A Draft contract is still editable and can receive requests
            var contract = new Contract
            {
                ContractId = 2,
                Status = ContractStatus.Draft
            };

            bool result = _service.IsContractEligibleForServiceRequest(contract);

            Assert.True(result);
        }

        [Fact]
        public void IsContractEligible_ExpiredContract_ReturnsFalse()
        {
            // Core business rule: expired contracts must block new requests
            var contract = new Contract
            {
                ContractId = 3,
                Status = ContractStatus.Expired,
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2021, 1, 1)
            };

            bool result = _service.IsContractEligibleForServiceRequest(contract);

            Assert.False(result);
        }

        [Fact]
        public void IsContractEligible_OnHoldContract_ReturnsFalse()
        {
            // Core business rule: on-hold contracts must also block requests
            var contract = new Contract
            {
                ContractId = 4,
                Status = ContractStatus.OnHold
            };

            bool result = _service.IsContractEligibleForServiceRequest(contract);

            Assert.False(result);
        }

        [Fact]
        public void IsContractEligible_NullContract_ReturnsFalse()
        {
            // Edge case: null should not crash — return false gracefully
            bool result = _service.IsContractEligibleForServiceRequest(null!);

            Assert.False(result);
        }

        // ════════════════════════════════════════════════════════════
        // INELIGIBILITY REASON MESSAGES
        // ════════════════════════════════════════════════════════════

        [Fact]
        public void GetIneligibilityReason_ExpiredContract_ContainsExpiredKeyword()
        {
            var contract = new Contract { Status = ContractStatus.Expired };

            string reason = _service.GetIneligibilityReason(contract);

            Assert.Contains("Expired", reason);
        }

        [Fact]
        public void GetIneligibilityReason_OnHoldContract_ContainsOnHoldKeyword()
        {
            var contract = new Contract { Status = ContractStatus.OnHold };

            string reason = _service.GetIneligibilityReason(contract);

            Assert.Contains("On Hold", reason);
        }

        [Fact]
        public void GetIneligibilityReason_ActiveContract_ReturnsEmptyString()
        {
            // An eligible contract has no ineligibility reason
            var contract = new Contract { Status = ContractStatus.Active };

            string reason = _service.GetIneligibilityReason(contract);

            Assert.Equal(string.Empty, reason);
        }

        [Fact]
        public void GetIneligibilityReason_NullContract_ReturnsNotFoundMessage()
        {
            string reason = _service.GetIneligibilityReason(null!);

            Assert.False(string.IsNullOrEmpty(reason));
        }

        // ════════════════════════════════════════════════════════════
        // DATE RANGE VALIDATION
        // ════════════════════════════════════════════════════════════

        [Fact]
        public void IsDateRangeValid_EndAfterStart_ReturnsTrue()
        {
            bool result = _service.IsDateRangeValid(
                new DateTime(2025, 1, 1),
                new DateTime(2027, 1, 1));

            Assert.True(result);
        }

        [Fact]
        public void IsDateRangeValid_EndBeforeStart_ReturnsFalse()
        {
            bool result = _service.IsDateRangeValid(
                new DateTime(2027, 1, 1),
                new DateTime(2025, 1, 1));

            Assert.False(result);
        }

        [Fact]
        public void IsDateRangeValid_SameStartAndEndDate_ReturnsFalse()
        {
            // A zero-duration contract is not valid
            var date = new DateTime(2025, 6, 1);

            bool result = _service.IsDateRangeValid(date, date);

            Assert.False(result);
        }

        [Fact]
        public void IsDateRangeValid_OneDayContract_ReturnsTrue()
        {
            // Even a single-day contract is technically valid
            bool result = _service.IsDateRangeValid(
                new DateTime(2025, 6, 1),
                new DateTime(2025, 6, 2));

            Assert.True(result);
        }

        // ════════════════════════════════════════════════════════════
        // START DATE REASONABLENESS
        // ════════════════════════════════════════════════════════════

        [Fact]
        public void IsStartDateReasonable_RecentDate_ReturnsTrue()
        {
            bool result = _service.IsStartDateReasonable(DateTime.Today);
            Assert.True(result);
        }

        [Fact]
        public void IsStartDateReasonable_FutureDate_ReturnsTrue()
        {
            bool result = _service.IsStartDateReasonable(DateTime.Today.AddMonths(6));
            Assert.True(result);
        }

        [Fact]
        public void IsStartDateReasonable_TooFarInPast_ReturnsFalse()
        {
            // A contract dated more than 1 year ago is suspicious
            bool result = _service.IsStartDateReasonable(DateTime.Today.AddYears(-2));
            Assert.False(result);
        }
    }
}