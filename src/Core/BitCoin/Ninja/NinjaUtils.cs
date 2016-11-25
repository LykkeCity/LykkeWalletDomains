using System;
using System.Linq;
using Core.Bitcoin;

namespace Core.BitCoin.Ninja
{
    public static class NinjaUtils
    {
        public static IBlockchainTransaction ConvertToBlockchainTransaction(this TransactionContract item)
        {
            if (IsColoredTransaction(item.ReceivedCoins, item.SpentCoins))
            {
                var receivedCoins = item.ReceivedCoins.GetColoredOnly(x => x.TransactionId == item.TransactionId);
                var spentCoins = item.SpentCoins.GetColoredOnly(x => x.TransactionId == item.TransactionId);

                //skip if received/spent coins has other tx id => it's just a related colored operation
                if (!receivedCoins.Any() && !spentCoins.Any())
                    return null;

                var clrTrans = item.ReceivedCoins?.FirstOrDefault() ?? item.SpentCoins.FirstOrDefault();

                return new BlockchainTransaction
                {
                    AssetId = clrTrans?.AssetId,
                    DateTime = item.Block?.BlockTime ?? item.FirstSeen,
                    TxId = clrTrans?.TransactionId,
                    Amount = receivedCoins.Sum(itm => itm.Quantity) - spentCoins.Sum(itm => itm.Quantity),
                    Confirmations = item.Block?.Confirmations ?? 0,
                    BlockId = item.Block?.BlockId,
                    Height = item.Block?.Height ?? 0
                };
            }

            double amount = item.ReceivedCoins?.Sum(x => x.Value) ?? 0;
            amount -= item.SpentCoins?.Sum(x => x.Value) ?? 0;

            return new BlockchainTransaction
            {
                DateTime = item.FirstSeen,
                TxId = item.TransactionId,
                Amount = amount,
                Confirmations = item.Block?.Confirmations ?? 0,
                BlockId = item.Block?.BlockId,
                Height = item.Block?.Height ?? 0

            };
        }

        public static IBlockchainTransaction ConvertToBlockchainTransaction(this BitcoinAddressOperation item, string address)
        {
            if (IsColoredTransaction(item.ReceivedCoins, item.SpentCoins))
            {
                var receivedCoins = item.ReceivedCoins.GetColoredOnly(x => x.TransactionId == item.TransactionId);
                var spentCoins = item.SpentCoins.GetColoredOnly(x => x.TransactionId == item.TransactionId);

                //skip if received/spent coins has other tx id => it's just a related colored operation
                if (!receivedCoins.Any() && !spentCoins.Any())
                    return null;

                var clrTrans = receivedCoins.FirstOrDefault() ?? spentCoins.FirstOrDefault();

                return new BlockchainTransaction
                {
                    AssetId = clrTrans.AssetId,
                    DateTime = item.FirstSeen,
                    TxId = clrTrans.TransactionId,
                    Amount = receivedCoins.Sum(itm => itm.Quantity) - spentCoins.Sum(itm => itm.Quantity),
                    Address = address,
                    Confirmations = item.Confirmations,
                    BlockId = item.BlockId,
                    Height = item.Height
                };
            }

            if (item.Amount != 0)
                return new BlockchainTransaction
                {
                    DateTime = item.FirstSeen,
                    TxId = item.TransactionId,
                    Amount = item.Amount,
                    Address = address,
                    Confirmations = item.Confirmations,
                    BlockId = item.BlockId,
                    Height = item.Height

                };

            return null;

        }

        private static bool IsColoredTransaction(BitCoinInOut[] receivedCoins, BitCoinInOut[] spentCoins)
        {
            return receivedCoins.GetColoredOnly().Any() || spentCoins.GetColoredOnly().Any();
        }

        private static BitCoinInOut[] GetColoredOnly(this BitCoinInOut[] coins,
            Func<BitCoinInOut, bool> whereClause = null)
        {
            var result = coins.Where(itm => !string.IsNullOrEmpty(itm.AssetId));
            if (whereClause != null)
                result = result.Where(whereClause);
            return result.ToArray();
        }

    }

}