﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.EventLogs
{
    public interface ITransferEvent : IBaseCashBlockchainOperation
    {
        string FromId { get; }
    }


    public class TransferEvent : ITransferEvent
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsHidden { get; set; }
        public string FromId { get; set; }
        public string AssetId { get; set; }
        public double Amount { get; set; }
        public string BlockChainHash { get; set; }
        public string Multisig { get; set; }
        public string TransactionId { get; set; }
        public string AddressFrom { get; set; }
        public string AddressTo { get; set; }
        public bool? IsSettled { get; set; }

        public static TransferEvent CreateNew(string clientId, string clientMultiSig, string fromId, string assetId, double amount,
            string transactionId, string addressFrom, string addressTo, bool isHidden = false)
        {
            return new TransferEvent
            {
                Id = Guid.NewGuid().ToString("N"),
                ClientId = clientId,
                DateTime = DateTime.UtcNow,
                FromId = fromId,
                AssetId = assetId,
                Amount = amount,
                TransactionId = transactionId,
                IsHidden = isHidden,
                AddressFrom = addressFrom,
                AddressTo = addressTo,
                Multisig = clientMultiSig,
                IsSettled = false
            };
        }

        public static TransferEvent CreateNewTransferAll(string clientId, string transactionId, string srcAddress)
        {
            return new TransferEvent
            {
                Id = Guid.NewGuid().ToString("N"),
                ClientId = clientId,
                DateTime = DateTime.UtcNow,
                TransactionId = transactionId,
                IsHidden = true,
                IsSettled = false,
                Multisig = srcAddress
            };
        }
    }

    public interface ITransferEventsRepository
    {
        Task<ITransferEvent> RegisterAsync(ITransferEvent transferEvent);

        Task<IEnumerable<ITransferEvent>> GetAsync(string clientId);
        Task<ITransferEvent> GetAsync(string clientId, string id);

        Task UpdateBlockChainHashAsync(string clientId, string id, string blockChainHash);

        Task SetBtcTransactionAsync(string clientId, string id, string btcTransaction);
        Task SetIsSettledAsync(string clientId, string id);

        Task<IEnumerable<ITransferEvent>> GetByHashAsync(string blockchainHash);
        Task<IEnumerable<ITransferEvent>> GetByMultisigAsync(string multisig);
        Task<IEnumerable<ITransferEvent>> GetByMultisigsAsync(string[] multisigs);
    }

}
