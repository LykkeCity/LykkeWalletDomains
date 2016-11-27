using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common;
using ProtoBuf;
using TcpSockets;

namespace LkeServices.MeConnector
{

    public class MatchingEngineSerializer : ITcpSerializer
    {

        private static readonly Dictionary<MeDataType, Type> Types = new Dictionary<MeDataType, Type>
        {
            [MeDataType.Ping] = typeof(MePingModel),
            [MeDataType.TheResponse] = typeof(TheResponseModel),
            [MeDataType.UpdateBalance] = typeof(MeCashInOutModel),
            [MeDataType.LimitOrder] = typeof(MeLimitOrderModel),
            [MeDataType.MarketOrder] = typeof(MeMarketOrderModel),
            [MeDataType.LimitOrderCancel] = typeof(MeLimitOrderCancelModel),
            [MeDataType.BalanceUpdate] = typeof(MeUpdateBalanceModel),
            [MeDataType.WalletCredentialsReload] = typeof(MeUpdateWalletCredsModel),

        };
        private static readonly Dictionary<Type, MeDataType> TypesReverse = new Dictionary<Type, MeDataType>();

        private readonly Dictionary<MeDataType, object> _instancesCache = new Dictionary<MeDataType, object>();

        static MatchingEngineSerializer()
        {
            foreach (var tp in Types)
                TypesReverse.Add(tp.Value, tp.Key);
        }

        public async Task<Tuple<object, int>> Deserialize(Stream stream)
        {
            const int headerSize = 5;

            var dataType = (MeDataType)await stream.ReadByteFromSocket();
            if (dataType == MeDataType.Ping)
                return new Tuple<object, int>(MePingModel.Instance, 1);

            var datalen = await stream.ReadIntFromSocket();
            if (datalen == 0)
            {

                lock (_instancesCache)
                {
                    if (!_instancesCache.ContainsKey(dataType))
                        _instancesCache.Add(dataType, Types[dataType].CreateUsingDefaultConstructor());

                    return new Tuple<object, int>(_instancesCache[dataType], headerSize);
                }

            }

            var data = await stream.ReadFromSocket(datalen);
            var memStream = new MemoryStream(data) {Position = 0};


            var result = Serializer.NonGeneric.Deserialize(Types[dataType], memStream);
            return new Tuple<object, int>(result, headerSize + datalen);

        }


        private static readonly byte[] PingPacket = {(byte)MeDataType.Ping};

        public byte[] Serialize(object data)
        {
            if (data is MePingModel)
                return PingPacket;


            var type = TypesReverse[data.GetType()];

            var memStream = new MemoryStream();
            Serializer.Serialize(memStream, data);
            var outData = memStream.ToArray();

            var outStream = new MemoryStream();
            outStream.WriteByte((byte)type);
            outStream.WriteInt(outData.Length);
            outStream.Write(outData, 0, outData.Length);

            return outStream.ToArray();
        }
    }
}