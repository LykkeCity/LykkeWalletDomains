using System.Threading.Tasks;
using Common;
using Core.BitCoin;
using Core.Clients;

namespace LkeServices.Clients
{
    public class SrvClientFinder
    {
        private readonly IPersonalDataRepository _personalDataRepository;
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;

        public SrvClientFinder(IPersonalDataRepository personalDataRepository, 
            IClientAccountsRepository clientAccountsRepository, IWalletCredentialsRepository walletCredentialsRepository)
        {
            _personalDataRepository = personalDataRepository;
            _clientAccountsRepository = clientAccountsRepository;
            _walletCredentialsRepository = walletCredentialsRepository;
        }

        public async Task<IPersonalData> FindClientAsync(string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
                return null;

            if (phrase.IsGuid())
                return await _personalDataRepository.GetAsync(phrase);

            if (phrase.IsValidEmail())
            {
                var client = await _clientAccountsRepository.GetByEmailAsync(phrase);
                if (client == null)
                    return null;

                return await _personalDataRepository.GetAsync(client.Id);
            }

            var phoneNum = phrase.GetDigitsAndSymbols();

            var result = await _personalDataRepository.ScanAndFindAsync(itm =>
                (!string.IsNullOrEmpty(itm.FullName) && itm.FullName.ToLower().Contains(phrase))
                || (!string.IsNullOrEmpty(itm.Email) && itm.Email.ToLower().Contains(phrase))
                || (!string.IsNullOrEmpty(itm.ContactPhone) && !string.IsNullOrEmpty(phoneNum)
                    && itm.ContactPhone.Contains(phoneNum))
                );


            if (result != null)
                return result;



            result = await FindByWalletAsync(phrase);

            return result;

        }


        private async Task<IPersonalData> FindByMultisigIndex(string phrase)
        {
            var clientId = await _walletCredentialsRepository.GetClientIdByMultisig(phrase);
            if (clientId == null)
                return null;
            return await _personalDataRepository.GetAsync(clientId);
        }

        private async Task<IPersonalData> FindByWalletAsync(string phrase)
        {
            var pd = await FindByMultisigIndex(phrase);

            if (pd != null)
                return pd;
            
            var result =
                await
                    _walletCredentialsRepository.ScanAndFind(
                        item => item.MultiSig == phrase || item.ColoredMultiSig == phrase || item.EthConversionWalletAddress == phrase);


            if (result == null)
                return null;

            return await _personalDataRepository.GetAsync(result.ClientId);
        }

    }
}
