using System;
using System.Threading.Tasks;
using Common;
using Core.Assets;
using Core.Kyc;

namespace LkeServices.Kyc
{
    public class SrvKycForAsset : ISrvKycForAsset
    {
        private readonly CachedDataDictionary<string, IAsset> _assets;
        private readonly IKycRepository _kycRepository;

        public SrvKycForAsset(CachedDataDictionary<string, IAsset> assets,
            IKycRepository kycRepository)
        {
            _assets = assets;
            _kycRepository = kycRepository;
        }

        public async Task<bool> IsKycNeeded(string clientId, string assetId)
        {
            if (string.IsNullOrEmpty(assetId))
                throw new ArgumentException(nameof(assetId));

            var asset = await _assets.GetItemAsync(assetId);

            if (asset == null)
                throw new ArgumentException(nameof(assetId));

            var userKycStatus = await _kycRepository.GetKycStatusAsync(clientId);

            return asset.KycNeeded && userKycStatus != KycStatus.Ok;
        }
    }
}
