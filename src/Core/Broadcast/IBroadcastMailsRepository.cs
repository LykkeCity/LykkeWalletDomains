using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Broadcast
{
    public enum BroadcastGroup
    {
        Kyc = 100,
        ClientSupport = 200,
        Errors = 300,
        Warnings = 400,
        Payments = 500,
        CompetitionPlatform = 600,
        BtcCashOuts = 700
    }

    public interface IBroadcastMail
    {
        string Email { get;}
        BroadcastGroup Group { get; }
    }

    public class BroadcastMail : IBroadcastMail
    {
        public string Email { get; set; }
        public BroadcastGroup Group { get; set; }
    }

    public interface IBroadcastMailsRepository
    {
        Task SaveAsync(IBroadcastMail broadcastMail);
        Task<IEnumerable<IBroadcastMail>> GetEmailsByGroup(BroadcastGroup group);
        Task DeleteAsync(IBroadcastMail broadcastMail);
        Task DeleteAsync(BroadcastGroup group, string email);
        bool RecordAlreadyExists(IBroadcastMail broadcastMail);
    }
}
