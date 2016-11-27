namespace Core.BitCoin.BlockchainCommands
{
    public static class CommandTypes
    {
        public const string GenerateNewWallet = "GenerateNewWallet";
        public const string CashIn = "CashIn";
        public const string CashOut = "CashOut";
        public const string Swap = "Swap";
        public const string Transfer = "Transfer";
        public const string GenerateRefundingTransaction = "GenerateRefundingTransaction";
        public const string GenerateFeeOutputs = "GenerateFeeOutputs";
        public const string GetFeeOutputsStatus = "GetFeeOutputsStatus";
        public const string OrdinaryCashOut = "OrdinaryCashOut";
        public const string GetCurrentBalance = "GetCurrentBalance";
        public const string UpdateAssets = "UpdateAssets";
        public const string TransferAllAssetsToAddress = "TransferAllAssetsToAddress";

        public static string GetCommandType(this string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                var i = message.IndexOf(':');
                return message.Substring(0, i);
            }

            return string.Empty;
        }

        public static string GetData(this string message)
        {
            var i = message.IndexOf(':');
            return message.Substring(i + 1, message.Length - i - 1);
        }
    }

    public class BaseCommandModel
    {
        public string TransactionId { get; set; }
    }

    public class CashIn : BaseCommandModel
    {
        public string MultisigAddress { get; set; }
        public float Amount { get; set; }
        public string Currency { get; set; }
    }

    public class CashOut : BaseCommandModel
    {
        public string MultisigAddress { get; set; }
        public float Amount { get; set; }
        public string Currency { get; set; }
        public string PrivateKey { get; set; }
    }

    public class OrdinaryCashOut : BaseCommandModel
    {
        public string MultisigAddress { get; set; }
        public float Amount { get; set; }
        public string Currency { get; set; }
        public string PrivateKey { get; set; }
        public string PublicWallet { get; set; }
    }

    public class Transfer : BaseCommandModel
    {
        public string SourceAddress { get; set; }
        public string SourcePrivateKey { get; set; }
        public string DestinationAddress { get; set; }
        public float Amount { get; set; }
        public string Asset { get; set; }
    }

    public class Swap : BaseCommandModel
    {
        public string MultisigCustomer1 { get; set; }
        public float Amount1 { get; set; }
        public string Asset1 { get; set; }
        public string MultisigCustomer2 { get; set; }
        public float Amount2 { get; set; }
        public string Asset2 { get; set; }
    }

    public class TransferAll : BaseCommandModel
    {
        public string SourceAddress { get; set; }
        public string SourcePrivateKey { get; set; }
        public string DestinationAddress { get; set; }
    }
}
