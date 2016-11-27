using System.Threading.Tasks;
using Common;
using Common.HttpRemoteRequests;
using Core.Accounts;
using Core.Settings;

namespace LkeServices.Account
{
    public class SrvReferralCodeFinder : ISrvReferralCodeFinder
    {
        private readonly BaseSettings _baseSettings;

        public SrvReferralCodeFinder(BaseSettings baseSettings)
        {
            _baseSettings = baseSettings;
        }

        public async Task<string> FindCode(string ip)
        {
            var result =
                (await
                    new HttpRequestClient().GetRequest(string.Format(_baseSettings.GetReferralCodeByIpServicePath, ip)))
                    .DeserializeJson<GetReferralCodeByIpServiceResponse>();

            return result?.ReferalCode;
        }
    }

    public class GetReferralCodeByIpServiceResponse
    {
        public string ReferalCode { get; set; }
    }
}
