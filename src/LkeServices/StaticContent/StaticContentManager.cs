using Core.StaticContent;

namespace LkeServices.StaticContent
{
    public class StaticContentManager : IStaticContentManager
    {
        public string GetWalletIconUrl(WalletIconSize iconSize, string walletName)
        {
            const string walletIconPathTemplate = "https://lkefiles.blob.core.windows.net:443/images/wallet_icons/{0}.png";

            char firstLetter = string.IsNullOrEmpty(walletName) ? 'W' : walletName[0]; //default letter 'W' - wallet

            string sizeSuffix = string.Empty;
            switch (iconSize)
            {
                case WalletIconSize.Medium:
                    sizeSuffix = "_md";
                    break;
                case WalletIconSize.Large:
                    sizeSuffix = "_lg";
                    break;
            }

            return string.Format(walletIconPathTemplate, $"{char.ToUpper(firstLetter)}{sizeSuffix}");
        }
    }
}
