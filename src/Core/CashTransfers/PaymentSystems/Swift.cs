namespace Core.CashTransfers.PaymentSystems
{
    public interface ISwiftFields
    {
        string Bic { get; }
        string AccNumber { get; }
        string AccName { get; }
        string Postcheck { get; }
    }

    public class Swift : ISwiftFields
    {
        public string Bic { get; set; }
        public string AccNumber { get; set; }
        public string AccName { get; set; }
        public string Postcheck { get; set; }
    }
}
