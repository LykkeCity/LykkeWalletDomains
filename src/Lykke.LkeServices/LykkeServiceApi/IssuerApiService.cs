using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Assets;
using Core.LykkeServiceApi;

namespace LkeServices.LykkeServiceApi
{
    public class IssuerApiService : IIssuerApiService
    {
        private readonly ILykkeServiceApiConnector _apiConnector;

        public IssuerApiService(ILykkeServiceApiConnector apiConnector)
        {
            _apiConnector = apiConnector;
        }

        public async Task<IEnumerable<Issuer>> GetMarginalIssuersAsync() {

            var requestUrl = "marginalissuer";

            return await _apiConnector.GetAsync<Issuer>(requestUrl);
        }
    }
}