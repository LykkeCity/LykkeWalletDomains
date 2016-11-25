using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Assets.Margin
{
    public interface IMarginAsset
    {
        string Id { get; }
        string Name { get; }
        string Symbol { get; }
        string IdIssuer { get; }
        int Accuracy { get; }
        double Multiplier { get; }
        double DustLimit { get; }
    }

    public class MarginAsset : IMarginAsset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string IdIssuer { get; set; }
        public int Accuracy { get; set; }
        public double Multiplier { get; set; }
        public double DustLimit { get; set; }

        public static MarginAsset CreateDefault()
        {
            return new MarginAsset();
        }
    }

    public interface IMarginAssetsRepository
    {
        Task RegisterAssetAsync(IMarginAsset asset);
        Task EditAssetAsync(string id, IMarginAsset asset);
        Task<IEnumerable<IMarginAsset>> GetAssetsAsync();
        Task<IMarginAsset> GetAssetAsync(string id);
    }
}