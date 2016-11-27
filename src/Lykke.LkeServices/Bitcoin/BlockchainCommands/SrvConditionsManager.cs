using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.Bitcoin;
using Core.BitCoin;
using Core.BitCoin.BlockchainCommands;
using Core.BitCoin.BlockchainCommands.Conditions;

namespace LkeServices.Bitcoin.BlockchainCommands
{
    public class SrvConditionsManagerSettings
    {
        public int DefaultConfirmationsLimit { get; set; }
        public int OrdinaryCashOutConfirmationsLimit { get; set; }
        public int CashInConfirmationsLimit { get; set; }
        public int TransferConfirmationsLimit { get; set; }
    }

    public class SrvConditionsManager
    {
        private readonly ICommandConditionsRepository _commandConditionsRepository;
        private readonly ISrvBlockchainReader _srvBlockchainReader;
        private readonly IBitCoinTransactionsRepository _bitCoinTransactionsRepository;
        private readonly SrvConditionsManagerSettings _srvConditionsManagerSettings;
        private readonly ISrvBlockchainHelper _srvBlockchainHelper;

        public SrvConditionsManager(ICommandConditionsRepository commandConditionsRepository, ISrvBlockchainReader srvBlockchainReader,
            IBitCoinTransactionsRepository bitCoinTransactionsRepository, SrvConditionsManagerSettings srvConditionsManagerSettings,
            ISrvBlockchainHelper srvBlockchainHelper)
        {
            _commandConditionsRepository = commandConditionsRepository;
            _srvBlockchainReader = srvBlockchainReader;
            _bitCoinTransactionsRepository = bitCoinTransactionsRepository;
            _srvConditionsManagerSettings = srvConditionsManagerSettings;
            _srvBlockchainHelper = srvBlockchainHelper;
        }

        public async Task<bool> AllConditionsMet(string transactionId, string commandType)
        {
            var conditionsForCmd = (await _commandConditionsRepository.GetConditions(transactionId)).ToArray();

            return !conditionsForCmd.Any() || await AllConditionsSatisfied(conditionsForCmd, commandType);
        }

        private async Task<bool> IsConfirmationsConditionMet(string context, string commandType)
        {
            if (!string.IsNullOrEmpty(context))
            {
                var contextObj = context.DeserializeJson<ConfirmationsConditionContext>();

                var blockchainHash = (await _bitCoinTransactionsRepository.FindByTransactionIdAsync(contextObj.TransactionId))?.BlockchainHash;

                if (!string.IsNullOrEmpty(blockchainHash))
                {
                    int confirmationsLimit = GetConfirmationsLimit(commandType);
                    var tx = await _srvBlockchainReader.GetByHashAsync(blockchainHash);
                    if (tx != null)
                        return tx.Confirmations >= confirmationsLimit;
                }
            }

            return false;
        }

        private async Task<bool> AllConditionsSatisfied(ICommandCondition[] conditions, string commandType)
        {
            foreach (var condition in conditions)
            {
                if (!await IsSatisfied(condition.Type, condition.Context, commandType))
                    return false;
            }
            return true;
        }

        private async Task<bool> IsSatisfied(string type, string context, string commandType)
        {
            switch (type)
            {
                case Types.ConfirmationsCondition:
                    return await IsConfirmationsConditionMet(context, commandType);
                //...
                default:
                    throw new ArgumentException("Unknown condition type");
            }
        }

        private int GetConfirmationsLimit(string commandType)
        {
            switch (commandType)
            {
                case CommandTypes.OrdinaryCashOut:
                case CommandTypes.TransferAllAssetsToAddress:
                {
                    return _srvConditionsManagerSettings.OrdinaryCashOutConfirmationsLimit;
                }
                case CommandTypes.CashIn:
                {
                    return _srvConditionsManagerSettings.CashInConfirmationsLimit;
                }
                case CommandTypes.Transfer:
                {
                    return _srvConditionsManagerSettings.TransferConfirmationsLimit;
                }
                default:
                {
                    return _srvConditionsManagerSettings.DefaultConfirmationsLimit;
                }
            }
        }
    }
}
