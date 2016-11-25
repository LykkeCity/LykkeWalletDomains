using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.Assets;
using Core.Assets.AssetGroup;
using Core.Exchange;
using Core.Settings;

namespace LkeServices.Assets.AssetGroups
{
    public class SrvAssetsHelper
    {
        private readonly IAssetGroupRepository _assetGroupRepository;
        private readonly IExchangeSettingsRepository _exchangeSettingsRepository;
        private readonly CachedDataDictionary<string, IAsset> _assetsDict;
        private readonly CachedDataDictionary<string, IAssetPair> _assetPairsDict;

        public SrvAssetsHelper(IAssetGroupRepository assetGroupRepository,
            IExchangeSettingsRepository exchangeSettingsRepository,
            CachedDataDictionary<string, IAsset> assetsDict,
            CachedDataDictionary<string, IAssetPair> assetPairsDict)
        {
            _assetGroupRepository = assetGroupRepository;
            _exchangeSettingsRepository = exchangeSettingsRepository;
            _assetsDict = assetsDict;
            _assetPairsDict = assetPairsDict;
        }

        public async Task<IAsset[]> GetAssetsForClient(string clientId, bool isIosDevice)
        {
            var result = (await _assetsDict.Values()).Where(x => !x.IsDisabled);

            var assetIdsForClient = await _assetGroupRepository.GetAssetIdsForClient(clientId, isIosDevice);

            if (assetIdsForClient != null)
                return result.Where(x => assetIdsForClient.Contains(x.Id)).ToArray();

            return result.ToArray();
        }


        public async Task<IAsset> GetBaseAssetForClient(string clientId, bool isIosDevice)
        {
            var assetsForClient = (await GetAssetsForClient(clientId, isIosDevice)).Where(x => x.IsBase);
            var exchangeSettings =
                await _exchangeSettingsRepository.GetOrDefaultAsync(clientId);

            var baseAsset = exchangeSettings.BaseAsset(isIosDevice);

            if (string.IsNullOrEmpty(baseAsset))
                baseAsset = assetsForClient.GetFirstAssetId();

            return await _assetsDict.GetItemAsync(baseAsset);
        }

        public async Task<IAssetPair[]> GetAssetsPairsForClient(string clientId, bool isIosDevice)
        {
            var assetsForClient = await GetAssetsForClient(clientId, isIosDevice);

            var baseAsset = await GetBaseAssetForClient(clientId, isIosDevice);

            return
                (await _assetPairsDict.Values()).WhichHaveAssets(baseAsset.Id)
                    .WhichConsistsOfAssets(assetsForClient.Select(x => x.Id).ToArray()).ToArray();
        }
    }
}
