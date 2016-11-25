namespace Core.BitCoin.BlockchainCommands.Conditions
{
    public static class Types
    {
        /// <summary>
        /// Command cannot be performed until needed confirmations limit
        /// </summary>
        public const string ConfirmationsCondition = "ConfirmationsCondition";
    }

    public interface IBaseCondition { }

    public class ConfirmationsConditionContext : IBaseCondition
    {
        public ConfirmationsConditionContext(string transactionId)
        {
            TransactionId = transactionId;
        }
        public string TransactionId { get; set; }
    }
}
