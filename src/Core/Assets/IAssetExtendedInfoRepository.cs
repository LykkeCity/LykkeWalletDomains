using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Assets
{

    public interface IAssetExtendedInfo
    {
        string Id { get; }
        string AssetClass { get; }
        string Description { get; }
        string NumberOfCoins { get; }
        string MarketCapitalization { get; }
        int PopIndex { get; }
        string AssetDescriptionUrl { get; }
        string FullName { get; }
    }


    public class AssetExtendedInfo : IAssetExtendedInfo
    {
        public string Id { get; set; }
        public string AssetClass { get; set; }
        public string Description { get; set; }
        public string NumberOfCoins { get; set; }
        public string MarketCapitalization { get; set; }
        public int PopIndex { get; set; }
        public string AssetDescriptionUrl { get; set; }
        public string FullName { get; set; }


        public static AssetExtendedInfo CreateDefault(string id)
        {
            return new AssetExtendedInfo
            {
                Id = id,
                PopIndex = 0,
                MarketCapitalization = string.Empty,
                Description = string.Empty,
                AssetClass = string.Empty,
                NumberOfCoins = string.Empty,
                AssetDescriptionUrl = string.Empty
            };
        }
    }

    public interface IAssetExtendedInfoRepository
    {

        Task SaveAsync(IAssetExtendedInfo src);
        Task<IAssetExtendedInfo> GetAssetExtendedInfoAsync(string id);
        Task<IEnumerable<IAssetExtendedInfo>> GetAllAsync();
    }


    public static class AssetExtendedInfoExt
    {
        public static async Task<IAssetExtendedInfo> GetAssetExtendedInfoOrDefaultAsync(this IAssetExtendedInfoRepository table, string id)
        {
            if (id == null)
                return AssetExtendedInfo.CreateDefault(null);
            var aei = await table.GetAssetExtendedInfoAsync(id);
            return aei ?? AssetExtendedInfo.CreateDefault(null);
        }
    }
}
