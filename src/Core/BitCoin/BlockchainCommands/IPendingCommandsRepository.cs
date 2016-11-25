using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.BitCoin.BlockchainCommands
{
    public interface IPendingCommand
    {
        string ClientId { get; }
    }

    public interface IPendingCommandsRepository
    {
        Task InsertOrReplace(string clientId);
        Task Remove(string clientId);
        Task<bool> HasPending(string clientId);
        Task ScanClientsAsync(Func<IEnumerable<IPendingCommand>, Task> chunk);
    }
}
