using System.Threading.Tasks;
using Core.Extensions;

namespace Core.BackgroundWorker
{
    public enum WorkType
    {
        SetEtheriumContract = 10,
        SetGeolocation = 20,
        SetReferralCode = 30,
        SetPin = 40,
        SetAntiFraudRecord = 50
    }

    public interface IBackgroundWorkRequestProducer
    {
        Task ProduceRequest<T>(WorkType workType, T context);
    }

    #region Contexts

    public class SetEtheriumContractContext
    {
        public SetEtheriumContractContext(string clientId)
        {
            ClientId = clientId;
        }

        public string ClientId { get; set; }
    }

    public class SetGeolocationContext
    {
        public SetGeolocationContext(string clientId, string ip)
        {
            ClientId = clientId;
            Ip = ip;
        }

        public string ClientId { get; set; }
        public string Ip { get; set; }
    }

    public class SetReferralCodeContext
    {
        public SetReferralCodeContext(string clientId, string ip)
        {
            ClientId = clientId;
            Ip = ip;
        }

        public string ClientId { get; set; }
        public string Ip { get; set; }
    }

    public class SetAntiFraudContext
    {
        public SetAntiFraudContext(string clientId, string ip, string email, string fullName, string phone)
        {
            ClientId = clientId;
            Ip = ip;
            Email = email;
            FullName = fullName;
            Phone = phone;
        }

        public string ClientId { get; set; }
        public string Ip { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
    }

    public class SetPinContext
    {
        public SetPinContext(string clientId, string pin)
        {
            ClientId = clientId;
            Pin = pin;
        }

        public string ClientId { get; set; }
        public string Pin { get; set; }
    }

    public class BackgroundWorkMessage
    {
        public WorkType WorkType { get; set; }
        public string ContextJson { get; set; }
    }

    public class BackgroundWorkMessage<T> : BackgroundWorkMessage
    {
        public BackgroundWorkMessage(WorkType workType, T contextObj)
        {
            ContextJson = contextObj.ToJson();
            WorkType = workType;
        }
    }

    #endregion
}
