using System.Threading.Tasks;
using Core;
using Core.Broadcast;
using Core.Clients;
using Core.Messages.Email;
using Core.Messages.Email.ContentGenerator.MessagesData;
using Core.Messages.Email.Sender;
using Core.PaymentSystems;

namespace LkeServices.PaymentSystems.PaymentOkNotificators
{
    public class PaymentOkEmailSender : IPaymentOkNotification
    {
        private readonly IEmailSender _emailSender;
        private readonly IPersonalDataRepository _personalDataRepository;

        public PaymentOkEmailSender(IEmailSender emailSender, IPersonalDataRepository personalDataRepository)
        {
            _emailSender = emailSender;
            _personalDataRepository = personalDataRepository;
        }

        public async Task NotifyAsync(IPaymentTransaction pt)
        {
            var pd = await _personalDataRepository.GetAsync(pt.ClientId);


            var body =
                $"Client: {pd.Email}, Payment system amount: {pt.AssetId} {pt.Amount.MoneyToStr()}, Deposited amount: {pt.DepositedAssetId} {pt.DepositedAmount}, PaymentSystem={pt.PaymentSystem}";

            await
                _emailSender.BroadcastEmailAsync(BroadcastGroup.Payments,
                    "Payment notification Ok", body);
        }
    }
}
