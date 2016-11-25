using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.CashTransfers
{
    public sealed class PaymentSystem
    {
        public static readonly PaymentSystem Swift = new PaymentSystem("SWIFT");

        private PaymentSystem(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator string(PaymentSystem paymentSystem)
        {
            return paymentSystem.ToString();
        }
    }

    public interface ICashOutRequest : IBaseCashOperation
    {
        string PaymentSystem { get; }
        string PaymentFields { get; }
    }

    public class SwiftCashOutRequest : ICashOutRequest
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string AssetId { get; set; }
        public string PaymentSystem { get; set; }
        public string PaymentFields { get; set; }
        public double Amount { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsHidden { get; set; }
    }

    public interface ICashOutBaseRepository
    {
        Task InsertRequestAsync(ICashOutRequest request);
        Task<IEnumerable<ICashOutRequest>> GetRequestsAsync(string clientId);
        Task<ICashOutRequest> GetAsync(string clientId, string requestId);
    }
}
