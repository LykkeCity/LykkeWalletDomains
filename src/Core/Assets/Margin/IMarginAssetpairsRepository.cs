using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Assets.Margin
{
    public interface IMarginAssetPair
    {
        string Id { get; }
        string Name { get; }
        string BaseAssetId { get; }
        string QuotingAssetId { get; }
        int Accuracy { get; }
        int InvertedAccuracy { get; }
    }

    public class MarginAssetPair : IMarginAssetPair
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BaseAssetId { get; set; }
        public string QuotingAssetId { get; set; }
        public int Accuracy { get; set; }
        public int InvertedAccuracy { get; set; }

        public static MarginAssetPair CreateDefault()
        {
            return new MarginAssetPair
            {
                Accuracy = 5,
            };
        }
    }

    public interface IMarginAssetPairsRepository
    {
        Task<IEnumerable<IMarginAssetPair>> GetAllAsync();
        Task<IMarginAssetPair> GetAsync(string id);
        Task AddAsync(IMarginAssetPair assetPair);
        Task EditAsync(string id, IMarginAssetPair assetPair);
    }
}