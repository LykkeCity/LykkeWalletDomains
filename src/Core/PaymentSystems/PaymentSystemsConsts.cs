using System;
using System.Collections.Generic;
using Core.PaymentSystems.CreditVoucher;

namespace Core.PaymentSystems
{
    public enum CashInPaymentSystem
    {
        Unknown, CreditVoucher, Bitcoin, Ethereum, Swift
    }


    public static class PaymentSystemsAndOtherInfo
    {
    
        public static readonly Dictionary<CashInPaymentSystem, Type> PsAndOtherInfoLinks = new Dictionary<CashInPaymentSystem, Type>
        {
            [CashInPaymentSystem.CreditVoucher] = typeof(CreditVoucherOtherPaymentInfo)
        };
    }
}
