using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Assets;
using Core.Bitcoin;

namespace Core.BitCoin.Ninja
{
    public class SrvNinjaBlockChainReader : ISrvBlockchainReader
    {
        private readonly string _url;
        private const string Base58Symbols = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        public SrvNinjaBlockChainReader(string url)
        {
            if (string.IsNullOrEmpty(url))
                _url = "/";
            else
                _url = url[url.Length - 1] == '/' ? url : url + '/';
        }

        private static async Task<string> DoRequest(string url)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            var webResponse = await webRequest.GetResponseAsync();
            using (var receiveStream = webResponse.GetResponseStream())
            {
                using (var sr = new StreamReader(receiveStream))
                {
                    return await sr.ReadToEndAsync();
                }

            }
        }

        private async Task<T> DoRequest<T>(string url)
        {
            var result = await DoRequest(url);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
        }

        public async Task<IEnumerable<IBlockchainTransaction>> GetTxsByAddressAsync(string address)
        {
            var data = await DoRequest<BtcAddressModel>($"{_url}balances/{address}?colored=true");

            var result = new List<IBlockchainTransaction>();

            foreach (var item in data.Operations)
            {
                var tx = item.ConvertToBlockchainTransaction(address);
                if (tx != null)
                    result.Add(tx);
            }

            return result;
        }

        public async Task<IBlockchainTransaction> GetByHashAsync(string hash)
        {

            try
            {
                var result = await DoRequest(_url + $"transactions/{hash}?colored=true");

                var contract = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionContract>(result);

                return contract.ConvertToBlockchainTransaction();

            }
            catch (Exception)
            {

                return null;
            }

        }

        public async Task<string> GetColoredMultisigAsync(string multisig)
        {
            try
            {
                var result = await DoRequest<WhatIsItContract>(_url + "whatisit/" + multisig);

                return result.ColoredAddress;
            }
            catch (Exception ex)
            {

                throw new Exception("Invalid Multisig: " + multisig);
            }
        }

        public async Task<bool> IsColoredAddress(string address)
        {
            var result = await DoRequest<WhatIsItContract>(_url + "whatisit/" + address);
            return result.Type == WhatIsItTypes.COLORED_ADDRESS;
        }

        public async Task<bool> IsValidAddress(string address, bool enableColored = false)
        {
            if (address.Any(x => !Base58Symbols.Contains(x)))
                return false;

            try
            {
                var result = await DoRequest<WhatIsItContract>(_url + "whatisit/" + address);

                bool valid = false;
                if (result != null)
                {
                    valid = result.Type == WhatIsItTypes.PUBKEY_ADDRESS ||
                    (result.Type == WhatIsItTypes.SCRIPT_ADDRESS && result.IsP2SH);
                    valid = enableColored ? valid || result.Type == WhatIsItTypes.COLORED_ADDRESS : valid;
                }

                return valid;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> GetUncoloredAdress(string address)
        {
            var result = await DoRequest<WhatIsItContract>(_url + "whatisit/" + address);

            if (result.Type == WhatIsItTypes.COLORED_ADDRESS)
                return result.UncoloredAddress;

            return address;
        }

        public async Task<IEnumerable<IBalanceRecord>> GetBalancesForAdress(string address, IAsset[] assets)
        {
            var result = await DoRequest($"{_url}balances/{address}/summary?colored=true");

            var contract = Newtonsoft.Json.JsonConvert.DeserializeObject<BalanceSummaryModel>(result);

            List<BalanceRecord> balances = new List<BalanceRecord>();

            var btc = assets.FirstOrDefault(x => x.Id == LykkeConstants.BitcoinAssetId);
            if (btc != null)
            {
                balances.Add(new BalanceRecord
                {
                    AssetId = LykkeConstants.BitcoinAssetId,
                    Balance = contract.Spendable.Amount * btc.Multiplier
                });
            }

            var assetsDict = assets.Where(x => !string.IsNullOrEmpty(x.BlockChainAssetId)).ToDictionary(x => x.BlockChainAssetId);

            balances.AddRange(contract.Spendable.Assets
                .Where(x => assetsDict.ContainsKey(x.AssetId))
                .Select(asset => new BalanceRecord
                {
                    AssetId = assetsDict[asset.AssetId].Id,
                    Balance = asset.Quantity * assetsDict[asset.AssetId].Multiplier
                }));

            return balances;
        }

        public async Task<double> GetBalancesSum(IEnumerable<string> addresses, IEnumerable<IAsset> assets, IAsset asset)
        {
            var getBalanceTasks = addresses.Select(address => GetBalance(address, assets, asset)).ToList();

            var balances = await Task.WhenAll(getBalanceTasks);

            return balances.Sum();
        }

        private async Task<double> GetBalance(string address, IEnumerable<IAsset> assets, IAsset asset)
        {
            var btc = assets.First(x => x.Id == LykkeConstants.BitcoinAssetId);

            var result = await DoRequest($"{_url}balances/{address}/summary?colored=true");
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<BalanceSummaryModel>(result);

            if (asset.BlockChainAssetId == null)
            {
                return model.Spendable.Amount * btc.Multiplier;
            }
            var balanceSummaryForAsset = model.Spendable.Assets.FirstOrDefault(x => x.AssetId == asset.BlockChainAssetId);

            if (balanceSummaryForAsset != null)
                return balanceSummaryForAsset.Quantity * asset.Multiplier;

            return 0;
        }
    }
}
