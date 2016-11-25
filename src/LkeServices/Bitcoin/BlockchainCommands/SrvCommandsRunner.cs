using System.Threading;
using System.Threading.Tasks;
using Common;
using Core.BitCoin.BlockchainCommands;
using Core.BitCoin.BlockchainCommands.Conditions;

namespace LkeServices.Bitcoin.BlockchainCommands
{
    public class SrvCommandsRunner
    {
        private readonly IBlockchainCommandsRepository _blockchainCommandsRepository;
        private readonly ICommandConditionsRepository _commandConditionsRepository;
        private readonly SrvConditionsManager _srvConditionsChecker;
        private readonly ICommandSender _commandSender;
        private readonly IPendingCommandsRepository _pendingCommandsRepository;
        private static readonly SemaphoreSlim Sl = new SemaphoreSlim(initialCount: 1);

        public SrvCommandsRunner(IBlockchainCommandsRepository blockchainCommandsRepository,
            ICommandConditionsRepository commandConditionsRepository, SrvConditionsManager srvConditionsChecker,
            ICommandSender commandSender, IPendingCommandsRepository pendingCommandsRepository)
        {
            _blockchainCommandsRepository = blockchainCommandsRepository;
            _commandConditionsRepository = commandConditionsRepository;
            _srvConditionsChecker = srvConditionsChecker;
            _commandSender = commandSender;
            _pendingCommandsRepository = pendingCommandsRepository;
        }

        public async Task TryExecuteTopCommand(string clientId)
        {
            var topCmd = await _blockchainCommandsRepository.GetTopRecord(clientId);

            if (topCmd != null)
            {
                await _commandConditionsRepository.AttachPendingToCommand(clientId, topCmd.TransactionId);

                if (await _srvConditionsChecker.AllConditionsMet(topCmd.TransactionId, topCmd.Command.GetCommandType()))
                {
                    await Sl.WaitAsync();

                    try
                    {
                        var topCmdToHandle = await _blockchainCommandsRepository.GetTopRecord(clientId);
                        if (topCmdToHandle.TransactionId != topCmd.TransactionId)
                            return;

                        if (!topCmdToHandle.DoNotSend)
                        {
                            await _commandSender.SendCommand(topCmd.Command);
                            await
                                _commandConditionsRepository.InsertPendingCondition(clientId,
                                    Types.ConfirmationsCondition,
                                    new ConfirmationsConditionContext(topCmd.TransactionId).ToJson());
                        }

                        await _blockchainCommandsRepository.DeleteTopRecord(clientId);
                        if (await _blockchainCommandsRepository.GetTopRecord(clientId) == null)
                            await _pendingCommandsRepository.Remove(clientId);
                    }
                    finally
                    {
                        // Release the thread
                        Sl.Release();
                    }
                }
            }
        }
    }
}
