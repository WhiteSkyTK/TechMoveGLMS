using System.Text.Json;
using TechMoveGLMS.Interfaces;
using TechMoveGLMS.Interfaces;

namespace TechMoveGLMS.Services
{
    public class ExchangeRateApiStrategy : ICurrencyStrategy
    {
        private readonly HttpClient _httpClient;

        // Dependency Injection of HttpClient prevents socket exhaustion
        public ExchangeRateApiStrategy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> ConvertUsdToZarAsync(decimal usdAmount)
        {
            try
            {
                // Note: In production, store the API URL in appsettings.json
                var response = await _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);

                var zarRate = result.GetProperty("rates").GetProperty("ZAR").GetDecimal();

                return Math.Round(usdAmount * zarRate, 2);
            }
            catch (Exception ex)
            {
                // Fallback logic or error logging goes here if API goes down
                throw new ApplicationException("Currency API is unavailable.", ex);
            }
        }
    }
}