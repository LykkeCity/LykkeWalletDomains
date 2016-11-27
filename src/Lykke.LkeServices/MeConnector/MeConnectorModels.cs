using Common;
using Core.Exchange;
using ProtoBuf;

namespace LkeServices.MeConnector
{
    public enum MeDataType
    {
        TheResponse, Ping, UpdateBalance, LimitOrder, MarketOrder, LimitOrderCancel, BalanceUpdate,
        WalletCredentialsReload = 20
    }

    public class MePingModel
    {
        public static readonly MePingModel Instance = new MePingModel();
    }

    [ProtoContract]
    public class TheResponseModel
    {
        [ProtoMember(1, IsRequired = true)]
        public long ProcessId { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public string CorrelationId { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public string RecordId { get; set; }
    }

    [ProtoContract]
    public class MeCashInOutModel
    {
        [ProtoMember(1, IsRequired = true)]
        public long Id { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public string ClientId { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public long DateTime { get; set; }

        [ProtoMember(4, IsRequired = true)]
        public string AssetId { get; set; }

        [ProtoMember(5, IsRequired = true)]
        public double Amount { get; set; }

        [ProtoMember(6, IsRequired = true)]
        public string CorrelationId { get; set; }

        [ProtoMember(7, IsRequired = true)]
        public bool SendToBitcoin { get; set; }

        public static MeCashInOutModel Create(long id, string clientId, string assetId, double amount, bool sendToBitcoin, string correlationId)
        {
            return new MeCashInOutModel
            {
                Id = id,
                DateTime = (long)System.DateTime.UtcNow.ToUnixTime(),
                ClientId = clientId,
                AssetId = assetId,
                Amount = amount,
                CorrelationId = correlationId,
                SendToBitcoin = sendToBitcoin
            };
        }
    }


    [ProtoContract]
    public class MeLimitOrderModel
    {
        [ProtoMember(1, IsRequired = true)]
        public long Id { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public long DateTime { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public string ClientId { get; set; }

        [ProtoMember(4, IsRequired = true)]
        public string AssetId { get; set; }

        [ProtoMember(5, IsRequired = true)]
        public double Volume { get; set; }

        [ProtoMember(6, IsRequired = true)]
        public double Price { get; set; }

        public static MeLimitOrderModel Create(long id, string clientId, string assetId, OrderAction orderAction, double volume, double price)
        {
            return new MeLimitOrderModel
            {
                Id = id,
                DateTime = (long)System.DateTime.UtcNow.ToUnixTime(),
                ClientId = clientId,
                AssetId = assetId,
                Volume = orderAction == OrderAction.Buy ? volume : -volume,
                Price = price
            };
        }
    }

    [ProtoContract]
    public class MeMarketOrderModel
    {
        [ProtoMember(1, IsRequired = true)]
        public long Id { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public long DateTime { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public string ClientId { get; set; }

        [ProtoMember(4, IsRequired = true)]
        public string AssetId { get; set; }

        [ProtoMember(5, IsRequired = true)]
        public double Volume { get; set; }

        [ProtoMember(6, IsRequired = true)]
        public bool Straight { get; set; }

        public static MeMarketOrderModel Create(long id, string clientId, string assetId, OrderAction orderAction, double volume, bool straight)
        {
            return new MeMarketOrderModel
            {
                Id = id,
                DateTime = (long)System.DateTime.UtcNow.ToUnixTime(),
                ClientId = clientId,
                AssetId = assetId,
                Volume = orderAction == OrderAction.Buy ? volume : -volume,
                Straight = straight
            };
        }
    }

    [ProtoContract]
    public class MeLimitOrderCancelModel
    {
        [ProtoMember(1, IsRequired = true)]
        public long Id { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public long OrderId { get; set; }


        public static MeLimitOrderCancelModel Create(long id, long orderId)
        {
            return new MeLimitOrderCancelModel
            {
                Id = id,
                OrderId = orderId
            };
        }

    }

    [ProtoContract]
    public class MeUpdateBalanceModel
    {
        [ProtoMember(1, IsRequired = true)]
        public long Id { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public string ClientId { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public string AssetId { get; set; }

        [ProtoMember(4, IsRequired = true)]
        public double Amount { get; set; }

        public static MeUpdateBalanceModel Create(long id, string clientId, string assetId, double amount)
        {
            return new MeUpdateBalanceModel
            {
                Id = id,
                ClientId = clientId,
                AssetId = assetId,
                Amount = amount
            };
        }
    }

    [ProtoContract]
    public class MeUpdateWalletCredsModel
    {
        [ProtoMember(1, IsRequired = true)]
        public long Id { get; set; }

        [ProtoMember(2, IsRequired = false)]
        public string ClientId { get; set; }


        public static MeUpdateWalletCredsModel Create(long id, string clientId)
        {
            return new MeUpdateWalletCredsModel
            {
                Id = id,
                ClientId = clientId
            };
        }
    }

}
