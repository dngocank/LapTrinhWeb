using BTL_PTTKYC.Models;

namespace BTL_PTTKYC.Services
{
    public interface IShippingService
    {
        ShippingQuoteResult CalculateShipping(ShippingQuoteRequest request);
        DistrictDistanceInfo GetDistrictInfo(string province, string? district);
        List<DistrictDistanceInfo> GetAllDistricts();
        decimal EstimateDistance(string province, string? district);
    }
}
