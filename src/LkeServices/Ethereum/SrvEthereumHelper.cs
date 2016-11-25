using System;
using System.Threading.Tasks;
using Common;
using Common.HttpRemoteRequests;
using Common.Log;
using Core.Ethereum;
using Core.Settings;

namespace LkeServices.Ethereum
{
    public class SrvEthereumHelper : ISrvEthereumHelper
    {
        private readonly BaseSettings _baseSettings;
        private readonly ILog _log;

        public SrvEthereumHelper(BaseSettings baseSettings, ILog log)
        {
            _baseSettings = baseSettings;
            _log = log;
        }

        public async Task<string> GetContract()
        {
            try
            {
                var result =
                    await new HttpRequestClient().Request(string.Empty, _baseSettings.Ethereum.ClientRegisterUrl);
                return result.DeserializeJson<GetContractModel>().Contract;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("SrvEthereumHelper", "GetContract", "", ex);
            }

            return null;
        }

        public async Task<string> GetContractByAddress(string address)
        {
            try
            {
                var result =
                    await new HttpRequestClient().GetRequest(string.Format(_baseSettings.Ethereum.GetContractForAddressUrl, address));
                return result.DeserializeJson<GetContractModel>().Contract;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("SrvEthereumHelper", "GetContractByAddress", "", ex);
            }

            return null;
        }

        public async Task AddAddressForContract(string contract, string address)
        {
            try
            {
                var model = new AddAddressForContractModel
                {
                    UserContract = contract,
                    UserWallet = address
                };

                await new HttpRequestClient().Request(model.ToJson(), _baseSettings.Ethereum.AddWalletUrl);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("SrvEthereumHelper", "AddAddressForContract", "", ex);
            }
        }

        #region Response Models

        public enum ErrorCodes
        {
            ContractPullWasEmpty = 1
        }

        public class Error
        {
            public ErrorCodes Code { get; set; }
            public string Msg { get; set; }
        }

        public class GetContractModel
        {
            public string Contract { get; set; }
        }

        public class AddAddressForContractModel
        {
            public string UserContract { get; set; }
            public string UserWallet { get; set; }
        }

        #endregion
    }
}
