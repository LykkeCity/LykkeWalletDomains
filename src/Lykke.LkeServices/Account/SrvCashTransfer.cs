using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Accounts;
using Core.BackOffice;
using Core.CashTransfers;
using Core.Clients;

namespace LkeServices.Account
{
    public class SrvCashTransfer
    {
        private readonly ICashOutAttemptRepository _cashOutAttemptRepository;
        private readonly ICashOutCancelledRepository _cashOutCancelledRepository;
        private readonly ICashOutDoneRepository _cashOutDoneRepository;
        private readonly IPersonalDataRepository _personalDataRepository;
        private readonly IMenuBadgesRepository _menuBadgesRepository;

        public SrvCashTransfer(IPersonalDataRepository personalDataRepository,
            IMenuBadgesRepository menuBadgesRepository, 
            ICashOutAttemptRepository cashOutAttemptRepository,
            ICashOutCancelledRepository cashOutCancelledRepository,
            ICashOutDoneRepository cashOutDoneRepository)
        {
            _personalDataRepository = personalDataRepository;
            _menuBadgesRepository = menuBadgesRepository;
            _cashOutAttemptRepository = cashOutAttemptRepository;
            _cashOutCancelledRepository = cashOutCancelledRepository;
            _cashOutDoneRepository = cashOutDoneRepository;
        }

        public async Task InsertAttemptRequest<T>(ICashOutRequest request, PaymentSystem paymentSystem, T paymentFields)
        {
            await _cashOutAttemptRepository.InsertRequestAsync(request, paymentSystem, paymentFields);
            await UpdateBadges();
        }

        public async Task InsertDoneRequest(ICashOutRequest request)
        {
            await _cashOutDoneRepository.InsertRequestAsync(request);
        }

        public async Task InsertCancelledRequest(ICashOutRequest request)
        {
            await _cashOutCancelledRepository.InsertRequestAsync(request);
        }

        public async Task<IEnumerable<IPersonalData>> GetClientsWithAttemptReq()
        {
            var clientIds = (await _cashOutAttemptRepository.GetAllAttempts())
                .OrderByDescending(x => x.DateTime)
                .GroupBy(x => x.ClientId)
                .Select(x => x.First())
                .Select(x => x.ClientId).ToArray();
            var pd = (await _personalDataRepository.GetAsync(clientIds)).ToDictionary(x => x.Id);

            return clientIds.Select(x => pd[x]);
        }

        #region Tools

        private async Task UpdateBadges()
        {
            var count = (await _cashOutAttemptRepository.GetAllAttempts()).Count();
            await _menuBadgesRepository.SaveBadgeAsync(MenuBadges.WithdrawRequest, count.ToString());
        }

        #endregion

    }
}
