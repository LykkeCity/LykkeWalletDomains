using System.Threading.Tasks;

namespace Core.PaymentSystems.CreditVoucher
{
    public interface IApiBankCards
    {
        Task<string> GetPaymentUrl(IPaymentTransaction bankCardOrder);
    }
}
