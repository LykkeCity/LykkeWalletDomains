using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Accounts
{
    public interface IPendingCashOperation
    {
        string Id { get; }
        string ClientId { get; }
        string CashOperationId { get; }
        string BlockchainHash { get; }
    }

    public class PendingCashOperation : IPendingCashOperation
    {
        public PendingCashOperation(string clientId, string cashOpId, string hash)
        {
            Id = Guid.NewGuid().ToString("N");
            ClientId = clientId;
            CashOperationId = cashOpId;
            BlockchainHash = hash;
        }

        public string Id { get; set; }
        public string ClientId { get; set; }
        public string CashOperationId { get; set; }
        public string BlockchainHash { get; set; }
    }

    public interface IPendingCashOperationsRepository
    {
        Task InsertAsync(IPendingCashOperation operation);
        Task<IPendingCashOperation> GetAsync(string clientId, string cashOpId);
        Task<IEnumerable<IPendingCashOperation>> GetAllAsync(string clientId);
        Task RemoveAsync(string clientId, string id);
    }
}
