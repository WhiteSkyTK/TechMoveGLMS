namespace TechMoveGLMS.Interfaces
{
    public interface ICurrencyStrategy
    {
        Task<decimal> ConvertUsdToZarAsync(decimal usdAmount);
    }
}