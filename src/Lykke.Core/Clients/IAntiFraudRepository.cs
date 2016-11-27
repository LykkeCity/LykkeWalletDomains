using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IAntiFraudRecord
    {
        string ClientId { get; }
        string Ip { get; }
        string Email { get; }
        string FullName { get; }
        string ContactPhone { get; }
        string ConnectedClientId { get; }
    }

    public class AntiFraudRecord : IAntiFraudRecord
    {
        public string ClientId { get; set; }
        public string Ip { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string ContactPhone { get; set; }
        public string ConnectedClientId { get; set; }

        public static AntiFraudRecord Create(string clientId, string ip, string email, string fullName, string phone)
        {
            return new AntiFraudRecord
            {
                ClientId = clientId,
                Ip = ip,
                Email = email,
                FullName = fullName,
                ContactPhone = phone,
                ConnectedClientId = ""
            };
        }
    }

    public interface IAntiFraudRepository
    {
        Task InsertRecordAsync(IAntiFraudRecord data);
        Task ConnectWithClient(string ip, string clientId, string connectedClientId);
        Task DisconnectClient(string ip, string clientId);
        Task<IEnumerable<IAntiFraudRecord>> GetSimilarRecordsAsync(string ip);
        Task<List<IAntiFraudRecord>> GetClientAntifraudRecordsAsync(string clientId);
    }
}