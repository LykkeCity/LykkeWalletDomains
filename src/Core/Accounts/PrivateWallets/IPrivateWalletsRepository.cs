using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.BitCoin;

namespace Core.Accounts.PrivateWallets
{
    public interface IPrivateWallet
    {
        string ClientId { get; }
        string WalletAddress { get; }
        string WalletName { get; }
        string EncodedPrivateKey { get; set; }
        bool? IsColdStorage { get; set; }
    }

    public class PrivateWallet : IPrivateWallet
    {
        public PrivateWallet() { }

        public PrivateWallet(IPrivateWallet privateWallet)
        {
            ClientId = privateWallet.ClientId;
            WalletAddress = privateWallet.WalletAddress;
            WalletName = privateWallet.WalletName;
            EncodedPrivateKey = privateWallet.EncodedPrivateKey;
            IsColdStorage = privateWallet.IsColdStorage;
        }

        public string ClientId { get; set; }
        public string WalletAddress { get; set; }
        public string EncodedPrivateKey { get; set; }
        public bool? IsColdStorage { get; set; }
        public string WalletName { get; set; }
    }

    public interface IPrivateWalletsRepository
    {
        Task CreateOrUpdateWallet(IPrivateWallet wallet);
        Task RemoveWallet(string address);

        /// <summary>
        /// Returns wallet by id from stored wallets, except default.
        /// To find among all wallets use extension GetAllPrivateWallets
        /// </summary>
        /// <param name="address">wallet address</param>
        /// <returns></returns>
        Task<IPrivateWallet> GetStoredWallet(string address);

        Task<IPrivateWallet> GetStoredWalletForUser(string address, string clientId);

        /// <summary>
        /// Returns all stored wallets, except default.
        /// To get all use extension GetPrivateWallet
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <returns>Private wallets enumeration</returns>
        Task<IEnumerable<IPrivateWallet>> GetStoredWallets(string clientId);
    }

    public static class PrivateWalletExt
    {
        public static async Task<IEnumerable<IPrivateWallet>> GetAllPrivateWallets(this IPrivateWalletsRepository repo, string clientId,
            IWalletCredentials walletCreds, string defaultWalletName)
        {
            var wallets = (await repo.GetStoredWallets(clientId)).ToList();

            IPrivateWallet defaultWallet = new PrivateWallet
            {
                ClientId = walletCreds.ClientId,
                WalletAddress = walletCreds.Address,
                WalletName = defaultWalletName
            };

            wallets.Add(defaultWallet);

            return wallets;
        }

        public static async Task<IPrivateWallet> GetPrivateWallet(this IPrivateWalletsRepository repo, string address,
            IWalletCredentials walletCreds, string defaultWalletName)
        {
            var wallet = await repo.GetStoredWallet(address);

            if (wallet == null && walletCreds.Address == address)
            {
                wallet = new PrivateWallet
                {
                    ClientId = walletCreds.ClientId,
                    WalletAddress = walletCreds.Address,
                    WalletName = defaultWalletName
                };
            }

            return wallet;
        }
    }
}
