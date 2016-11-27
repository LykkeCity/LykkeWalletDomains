using System;
using System.Net;
using System.Threading.Tasks;
using Common;
using Core.Exchange;
using TcpSockets;

namespace LkeServices.MeConnector
{

    public class TcpOrderSocketService : ITcpClientService, ISocketNotifyer
    {
        private readonly TasksManager<long, TheResponseModel> _tasksManager;

        public TcpOrderSocketService(TasksManager<long, TheResponseModel> tasksManager)
        {
            _tasksManager = tasksManager;
        }

        public Task HandleDataFromSocket(object data)
        {
            var theResponse = data as TheResponseModel;

            if (theResponse != null)
            {
                _tasksManager.Compliete(theResponse.ProcessId, theResponse);
                Console.WriteLine("Response ProcessId: "+theResponse.ProcessId);
            }


            return Task.FromResult(0);
        }

        public Func<object, Task> SendDataToSocket { get; set; }
        public string ContextName => "TcpSocket";
        public object GetPingData()
        {
            return MePingModel.Instance;
        }

        public Task Connect()
        {
            return Task.FromResult(0);
        }

        public Task Disconnect()
        {
            _tasksManager.SetExceptionsToAll(new Exception("Socket disconnected"));
            return Task.FromResult(0);
        }
    }

    public class TcpClientMatchingEngineConnector : IMatchingEngineConnector
    {
        private readonly TasksManager<long, TheResponseModel> _tasksManager = new TasksManager<long, TheResponseModel>();  

        private readonly ClientTcpSocket<MatchingEngineSerializer, TcpOrderSocketService> _clientTcpSocket;

        private readonly object _lockObject = new object();
        private long _currentNumber = 1;

        private TcpOrderSocketService _tcpOrderSocketService;

        private long GetNextRequestId()
        {
            lock (_lockObject)
                return _currentNumber++;
        }

        public TcpClientMatchingEngineConnector(IPEndPoint ipEndPoint, ISocketLog socketLog = null)
        {
            _clientTcpSocket = new ClientTcpSocket<MatchingEngineSerializer, TcpOrderSocketService>(
                socketLog,
                ipEndPoint,
                3000,
                () =>
                {
                    _tcpOrderSocketService = new TcpOrderSocketService(_tasksManager);
                    return _tcpOrderSocketService;
                });
        }


        public async Task<string> HandleMarketOrderAsync(string clientId, string assetId, OrderAction orderAction, double volume, bool straight)
        {
            var id = GetNextRequestId();

            var marketOrderModel = MeMarketOrderModel.Create(id, clientId, assetId, orderAction, volume, straight);
            var resultTask = _tasksManager.Add(id);
            await _tcpOrderSocketService.SendDataToSocket(marketOrderModel);
            var result = await resultTask;

            return result.RecordId;
        }

        public async Task HandleLimitOrderAsync(string clientId, string assetId, OrderAction orderAction, double volume, double price)
        {
            var id = GetNextRequestId();

            var limitOrderModel = MeLimitOrderModel.Create(id, clientId, assetId, orderAction, volume, price);
            var resultTask = _tasksManager.Add(id);
            await _tcpOrderSocketService.SendDataToSocket(limitOrderModel);
            await resultTask;
        }

        public async Task<CashInOutResponse> CashInOutBalanceAsync(string clientId, string assetId, double balanceDelta, bool sendToBitcoin, string corelationId)
        {
            var id = GetNextRequestId();

            var updateBalanceModel = MeCashInOutModel.Create(id, clientId, assetId, balanceDelta, sendToBitcoin, corelationId);
            var resultTask = _tasksManager.Add(id);
            await _tcpOrderSocketService.SendDataToSocket(updateBalanceModel);
            var result = await resultTask;

            return new CashInOutResponse
            {
                RecordId = result.RecordId,
                CorrelationId = result.CorrelationId
            };

        }

        public async Task UpdateBalanceAsync(string clientId, string assetId, double value)
        {
            var id = GetNextRequestId();
            var model = MeUpdateBalanceModel.Create(id, clientId, assetId, value);

            var resultTask = _tasksManager.Add(id);
            await _tcpOrderSocketService.SendDataToSocket(model);
            await resultTask;
        }

        public async Task CancelLimitOrderAsync(int orderId)
        {
            var id = GetNextRequestId();
            var cancelOrderModel = MeLimitOrderCancelModel.Create(id, orderId);
            var resultTask = _tasksManager.Add(id);
            await _tcpOrderSocketService.SendDataToSocket(cancelOrderModel);
            await resultTask;
        }

        public async Task<bool> UpdateWalletCredsForClient(string clientId)
        {
            var id = GetNextRequestId();
            var updateWalletCredsModel = MeUpdateWalletCredsModel.Create(id, clientId);
            var resultTask = _tasksManager.Add(id);
            await _tcpOrderSocketService.SendDataToSocket(updateWalletCredsModel);
            var result = await resultTask;

            return result.ProcessId == id;
        }

        public void Start()
        {
            _clientTcpSocket.Start();
        }

        public bool IsConnected => _clientTcpSocket.Connected;

        public SocketStatistic SocketStatistic => _clientTcpSocket.SocketStatistic;


    }
}