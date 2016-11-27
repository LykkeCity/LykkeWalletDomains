using System;

namespace Core
{
    /// <summary>
    /// Base cash operation
    /// </summary>
    public interface IBaseCashOperation
    {
        /// <summary>
        /// Record Id
        /// </summary>
        string Id { get; }

        string AssetId { get; }

        string ClientId { get; }

        double Amount { get; }

        DateTime DateTime { get; }

        bool IsHidden { get; }
    }

    /// <summary>
    /// Base cash blockchain operation
    /// E.g. cash in, cash out, trade, transfer
    /// </summary>
    public interface IBaseCashBlockchainOperation : IBaseCashOperation
    {
        string BlockChainHash { get; }

        string Multisig { get; }

        /// <summary>
        /// Bitcoin queue record id (BitCointTransaction)
        /// </summary>
        string TransactionId { get; }

        string AddressFrom { get; set; }

        string AddressTo { get; set; }

        bool? IsSettled { get; set; }
    }
}
