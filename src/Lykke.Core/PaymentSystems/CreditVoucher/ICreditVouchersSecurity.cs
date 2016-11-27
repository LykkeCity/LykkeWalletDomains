using System.Collections.Generic;

namespace Core.PaymentSystems.CreditVoucher
{
    public interface ICreditVouchersSecurity
    {
        string CalculateHeaderCheckSum(IDictionary<string, string> inputProperties);
        string CalculateCheckSum(IDictionary<string, string> inputProperties, string header);
    }
}
