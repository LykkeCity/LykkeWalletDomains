using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Assets;
using Core.Exchange;

namespace LkeServices.Exchange
{

    public class OrderBookResult
    {
        public class OrderBookLine
        {
            public double Price { get; set; }
            public double Volume { get; set; }
            public OrderAction OrderAction { get; set; }
            public static OrderBookLine Create(double price, double volume, OrderAction orderAction)
            {
                return new OrderBookLine
                {
                    Price = price,
                    Volume = volume,
                    OrderAction = orderAction
                };
            }

        }

        private readonly List<OrderBookLine> _orderBookLines = new List<OrderBookLine>(); 

        public IAssetPair AssetPair { get; private set; }

        internal void AddLimitOrder(ILimitOrder limitOrder)
        {
            var limitOrderAction = limitOrder.OrderAction();
            var orderBookLine = _orderBookLines.FirstOrDefault(itm => itm.OrderAction == limitOrderAction && AssetPair.PricesAreTheSame(itm.Price, limitOrder.Price));

            if (orderBookLine == null)
            {
                orderBookLine = OrderBookLine.Create(limitOrder.Price, limitOrder.RemainingVolume, limitOrderAction);
                _orderBookLines.Add(orderBookLine);
            }
            else
                orderBookLine.Volume += limitOrder.RemainingVolume;
        }


        public IEnumerable<OrderBookLine> GetOrderBookLines()
        {
            return _orderBookLines.OrderByDescending(itm => itm.Price);
        }

        public static OrderBookResult Create(IAssetPair assetPair)
        {
            return new OrderBookResult
            {
                AssetPair = assetPair
            };
        }
    }



    public class SrvOrderBookBuilder
    {
        private readonly IAssetPairsRepository _assetPairsRepository;
        private readonly IActiveLimitOrdersRepository _activeLimitOrdersRepository;

        public SrvOrderBookBuilder(IAssetPairsRepository assetPairsRepository, IActiveLimitOrdersRepository activeLimitOrdersRepository)
        {
            _assetPairsRepository = assetPairsRepository;
            _activeLimitOrdersRepository = activeLimitOrdersRepository;
        }

        public async Task<IEnumerable<OrderBookResult>> BuildOrderBook()
        {

            var assetPairs = (await _assetPairsRepository.GetAllAsync()).ToDictionary(itm => itm.Id);
            var limitOrders = await _activeLimitOrdersRepository.GetAsync();

            var result = new Dictionary<string, OrderBookResult>();


            foreach (var limitOrder in limitOrders)
            {
                if (!assetPairs.ContainsKey(limitOrder.AssetPairId))
                    continue;

                var assetPair = assetPairs[limitOrder.AssetPairId];

                if (!result.ContainsKey(limitOrder.AssetPairId) )
                    result.Add(limitOrder.AssetPairId, OrderBookResult.Create(assetPair));

                var orderbook = result[limitOrder.AssetPairId];
                orderbook.AddLimitOrder(limitOrder);
            }


            return result.Values;
        }
    }
}
