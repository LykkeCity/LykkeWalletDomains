using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.BitCoin.BlockchainCommands
{
    public interface ICommandCondition
    {
        string TransactionId { get; }
        string Type { get; }
        string Context { get; }
    }

    public interface ICommandConditionsRepository
    {
        Task InsertCondition(string transactionId, string type, string context);

        Task<IEnumerable<ICommandCondition>> GetConditions(string transactionId);

        Task InsertPendingCondition(string clientId, string type, string context);

        /// <summary>
        /// Attaches pending conditions to command
        /// </summary>
        /// <param name="clientId">client id with pending conditions</param>
        /// <param name="transactionId">command id to attach</param>
        /// <returns></returns>
        Task AttachPendingToCommand(string clientId, string transactionId);
    }
}
