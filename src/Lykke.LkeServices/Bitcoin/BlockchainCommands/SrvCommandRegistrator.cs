using System;
using System.Threading.Tasks;
using Core.BitCoin.BlockchainCommands;

namespace LkeServices.Bitcoin.BlockchainCommands
{
    public class SrvCommandRegistrator
    {
        private readonly IBlockchainCommandsRepository _blockchainCommandsRepository;
        private readonly IPendingCommandsRepository _pendingCommandsRepository;

        public SrvCommandRegistrator(IBlockchainCommandsRepository blockchainCommandsRepository,
            IPendingCommandsRepository pendingCommandsRepository)
        {
            _blockchainCommandsRepository = blockchainCommandsRepository;
            _pendingCommandsRepository = pendingCommandsRepository;
        }

        /// <summary>
        /// Registers new command pending to broadcast
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="transactionId">BitCoin transaction id</param>
        /// <param name="command">Command JSON</param>
        /// <param name="dateTime">Command insertion date time</param>
        public async Task RegisterNewCommand(string transactionId, string clientId, string command, DateTime dateTime)
        {
            await _blockchainCommandsRepository.InsertCommand(transactionId, clientId, command, dateTime);
            await _pendingCommandsRepository.InsertOrReplace(clientId);
        }

        /// <summary>
        /// Register new fake command which will not be broadcasted.
        /// </summary>
        /// <returns>Fake command id</returns>
        public async Task<string> RegisterNewDoNotSendCommand(string transactionId, string clientId, DateTime dateTime)
        {
            var id = await _blockchainCommandsRepository.InsertDoNotSendCommand(transactionId, clientId, dateTime);
            await _pendingCommandsRepository.InsertOrReplace(clientId);

            return id;
        }
    }
}
