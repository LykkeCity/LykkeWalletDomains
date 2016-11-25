using System.Threading.Tasks;

namespace Core.Feed
{
    public interface IFeedHoursHistory
    {
        string AssetPairId { get; }
        double[] Changes { get; }
    }

    public interface IFeedHoursHistoryRepository
    {
        Task<IFeedHoursHistory> GetAsync(string assetPairId);
    }
}
