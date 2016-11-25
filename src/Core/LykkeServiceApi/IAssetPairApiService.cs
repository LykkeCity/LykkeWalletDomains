using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Assets;

namespace Core.LykkeServiceApi
{
    public interface IAssetPairApiService
    {
        Task<IEnumerable<AssetPair>> GetMarginalAssetPairsAsync();
    }
}