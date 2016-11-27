using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.EventLogs
{
    public interface IFailedTransaction
    {
        string ClientIds { get; set; }
        string TransactionId { get; set; }
        DateTime DateTime { get; set; }
    }

    public interface IFailedTransactionRepository
    {
        Task InsertAsync(string transactionId, string[] clientIds);
        Task<IEnumerable<IFailedTransaction>> GetAllAsync();
        Task RemoveByTransactionAsync(string transactionId);
    }
}
