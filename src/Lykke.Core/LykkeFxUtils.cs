
using System.Net.Sockets;
using System.Threading.Tasks;
using Core.Assets;
using Core.Bitcoin;
using Core.BitCoin;
using Core.Exchange;

namespace Core
{
    public static class LykkeFxUtils
    {

        public static OrderAction GetMeOrderAction(this OrderAction orderAction, IAssetPair assetPair, string baseAsset)
        {

            if (assetPair.QuotingAssetId == baseAsset)
                return orderAction;

            return orderAction == OrderAction.Buy ? OrderAction.Sell : OrderAction.Buy;

        }
    }
}
