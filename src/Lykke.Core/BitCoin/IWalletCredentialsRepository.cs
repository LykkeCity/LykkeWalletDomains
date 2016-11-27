﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.BitCoin
{
    public interface IWalletCredentials
    {
        string ClientId { get; }
        string Address { get; }
        string PublicKey { get; }
        string PrivateKey { get; }
        string MultiSig { get; }
        string ColoredMultiSig { get; }
        bool PreventTxDetection { get; }
        string EncodedPrivateKey { get; }

        /// <summary>
        /// Conversion wallet is used for accepting BTC deposit and transfering needed LKK amount
        /// </summary>
        string BtcConvertionWalletPrivateKey { get; set; }
        string BtcConvertionWalletAddress { get; set; }

        /// <summary>
        /// Eth contract for user
        /// </summary>
        //ToDo: rename field to EthContract and change existing records
        string EthConversionWalletAddress { get; set; }
        string EthAddress { get; set; }
        string EthPublicKey { get; set; }
    }

    public class WalletCredentials : IWalletCredentials
    {
        public string ClientId { get; set; }
        public string Address { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string MultiSig { get; set; }
        public string ColoredMultiSig { get; set; }
        public bool PreventTxDetection { get; set; }
        public string EncodedPrivateKey { get; set; }

        /// <summary>
        /// Conversion wallet is used for accepting BTC deposit and transfering needed LKK amount
        /// </summary>
        public string BtcConvertionWalletPrivateKey { get; set; }
        public string BtcConvertionWalletAddress { get; set; }

        //EthContract in fact. ToDo: rename
        public string EthConversionWalletAddress { get; set; }
        public string EthAddress { get; set; }
        public string EthPublicKey { get; set; }

        public static WalletCredentials Create(string clientId, string address, string privateKey,
            string multisig, string coloredMultiSig, string btcConvertionWalletPrivateKey,
            string btcConvertionWalletAddress, bool preventTxDetection = false,
            string encodedPk = "", string pubKey = "")
        {
            return new WalletCredentials
            {
                ClientId = clientId,
                Address = address,
                PublicKey = pubKey,
                PrivateKey = privateKey,
                MultiSig = multisig,
                ColoredMultiSig = coloredMultiSig,
                PreventTxDetection = preventTxDetection,
                EncodedPrivateKey = encodedPk,
                BtcConvertionWalletPrivateKey = btcConvertionWalletPrivateKey,
                BtcConvertionWalletAddress = btcConvertionWalletAddress
            };
        }

        public static WalletCredentials Create(IWalletCredentials src)
        {
            return new WalletCredentials
            {
                ClientId = src.ClientId,
                Address = src.Address,
                PrivateKey = src.PrivateKey,
                MultiSig = src.MultiSig,
                ColoredMultiSig = src.ColoredMultiSig,
                PreventTxDetection = src.PreventTxDetection,
                EncodedPrivateKey = src.EncodedPrivateKey,
                PublicKey = src.PublicKey,
                BtcConvertionWalletPrivateKey = src.BtcConvertionWalletPrivateKey,
                BtcConvertionWalletAddress = src.BtcConvertionWalletAddress,
                EthConversionWalletAddress = src.EthConversionWalletAddress,
                EthAddress = src.EthAddress,
                EthPublicKey = src.EthPublicKey
            };
        }
    }

    public interface IWalletCredentialsRepository
    {
        /// <summary>
        /// Сохранить сгенеренные данные по бит коину
        /// </summary>
        /// <param name="walletCredentials"></param>
        /// <returns></returns>
        Task SaveAsync(IWalletCredentials walletCredentials);

        Task MergeAsync(IWalletCredentials walletCredentials);

        Task<IWalletCredentials> GetAsync(string clientId);

        Task<IWalletCredentials> GetByEthConversionWalletAsync(string ethWallet);

        Task<string> GetClientIdByMultisig(string multisig);

        Task SetPreventTxDetection(string clientId, bool value);

        Task SetEncodedPrivateKey(string clientId, string encodedPrivateKey);

        Task SetEthConversionWallet(string clientId, string contract);

        Task SetEthFieldsWallet(string clientId, string contract, string address, string pubKey);

        Task<IWalletCredentials> ScanAndFind(Func<IWalletCredentials, bool> item);

        Task ScanAllAsync(Func<IEnumerable<IWalletCredentials>, Task> chunk);
    }

    public interface IWalletCredentialsHistoryRepository
    {
        Task InsertHistoryRecord(IWalletCredentials oldWalletCredentials);
        Task<IEnumerable<string>> GetPrevMultisigsForUser(string clientId);
    }

    public static class WalletCredentialsExt
    {
        public static async Task<string[]> GetAllClientMultisigs(this IWalletCredentialsRepository walletCredsRepo,
            IWalletCredentialsHistoryRepository walletHistoryCredsRepo, string clientId)
        {
            var currentCreds = await walletCredsRepo.GetAsync(clientId);
            var prevMultisigs = await walletHistoryCredsRepo.GetPrevMultisigsForUser(clientId);

            var res = new List<string>();
            res.Add(currentCreds.MultiSig);
            res.AddRange(prevMultisigs);

            return res.Distinct().ToArray();
        }
    }
}