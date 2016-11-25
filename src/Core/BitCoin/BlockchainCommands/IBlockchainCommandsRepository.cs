using System;
using System.Threading.Tasks;

namespace Core.BitCoin.BlockchainCommands
{
    public interface IBlockchainCommand
    {
        string TransactionId { get; }
        string ClientId { get; }
        string Command { get; }
        DateTime DateTime { get; }
        bool DoNotSend { get; }
    }

    public interface IBlockchainCommandsRepository
    {
        Task InsertCommand(string transactionId, string clientId, string command, DateTime dateTime);

        /// <summary>
        /// Inserts command which will not be broadcasted.
        /// E.g. for swap, which affect two client and should be brodcasted only once (for first client).
        /// For second we create fake command which is dependant on confirmations for real command
        /// </summary>
        /// <returns>Fake command id</returns>
        Task<string> InsertDoNotSendCommand(string transactionId, string clientId, DateTime dateTime);

        Task<IBlockchainCommand> GetTopRecord(string clientId);

        Task<IBlockchainCommand> GetRecord(string clientId, string id);

        Task DeleteTopRecord(string clientId);
    }
}
