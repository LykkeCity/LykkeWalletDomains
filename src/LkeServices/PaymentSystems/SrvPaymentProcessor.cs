using System.Threading.Tasks;
using Core;
using Core.Assets;
using Core.PaymentSystems;
using LkeServices.Bitcoin;

namespace LkeServices.PaymentSystems
{
    public interface IPaymentOkNotification
    {
        Task NotifyAsync(IPaymentTransaction paymentTransaction);
    }


    public class SrvPaymentProcessor
    {
        private readonly SrvBitcoinCommandProducer _srvBitcoinCommandProducer;
        private readonly IPaymentTransactionsRepository _paymentTransactionsRepository;
        private readonly IPaymentTransactionEventsLog _paymentTransactionEventsLog;
        private readonly IPaymentOkNotification[] _paymentOkNotifications;


        public SrvPaymentProcessor(SrvBitcoinCommandProducer srvBitcoinCommandProducer,
            IPaymentTransactionsRepository paymentTransactionsRepository, IPaymentTransactionEventsLog paymentTransactionEventsLog,
            IPaymentOkNotification[] paymentOkNotifications)
        {
            _srvBitcoinCommandProducer = srvBitcoinCommandProducer;
            _paymentTransactionsRepository = paymentTransactionsRepository;
            _paymentTransactionEventsLog = paymentTransactionEventsLog;
            _paymentOkNotifications = paymentOkNotifications;
        }

        public async Task<bool> NotifyAsOkAsync(string transactionId, string paymentSystemTransactionId, string sourceClientId, string who)
        {
            var pt =
                await
                    _paymentTransactionsRepository.StartProcessingTransactionAsync(transactionId,
                        paymentSystemTransactionId);

            if (pt == null)
            {
                await
                    _paymentTransactionEventsLog.WriteAsync(PaymentTransactionLogEvent.Create(transactionId, "N/A",
                        "Transaction not found or already processed", who));
                return false;
            }

            await _srvBitcoinCommandProducer.TransferBetweenClientsWithNotification(pt.ClientId, sourceClientId,
                pt.Amount, pt.AssetId);

            var resultTransaction = await
                _paymentTransactionsRepository.SetAsOkAsync(transactionId, pt.Amount, null);

            await
                _paymentTransactionEventsLog.WriteAsync(PaymentTransactionLogEvent.Create(transactionId, "",
                    "Transaction processed as Ok", who));

            foreach (var notification in _paymentOkNotifications)
                await notification.NotifyAsync(resultTransaction);

            return true;
        }

    }
}