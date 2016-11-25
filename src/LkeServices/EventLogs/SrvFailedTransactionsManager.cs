using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.BackOffice;
using Core.EventLogs;

namespace LkeServices.EventLogs
{
    public class SrvFailedTransactionsManager
    {
        private readonly IFailedTransactionRepository _failedTransactionRepository;
        private readonly IMenuBadgesRepository _menuBadgesRepository;

        public SrvFailedTransactionsManager(IFailedTransactionRepository failedTransactionRepository,
            IMenuBadgesRepository menuBadgesRepository)
        {
            _failedTransactionRepository = failedTransactionRepository;
            _menuBadgesRepository = menuBadgesRepository;
        }

        public async Task InsertFailedTransaction(string transactionId, string[] clientIds)
        {
            await _failedTransactionRepository.InsertAsync(transactionId, clientIds);
            await UpdateBadges();
        }

        public async Task RemoveFailedTransaction(string transactionId)
        {
            await _failedTransactionRepository.RemoveByTransactionAsync(transactionId);
            await UpdateBadges();
        }

        public Task<IEnumerable<IFailedTransaction>>  GetAllAsync()
        {
            return _failedTransactionRepository.GetAllAsync();
        }

        private async Task UpdateBadges()
        {
            var count = (await _failedTransactionRepository.GetAllAsync()).Count();
            await _menuBadgesRepository.SaveBadgeAsync(MenuBadges.FailedTransaction, count.ToString());
        }
    }
}
