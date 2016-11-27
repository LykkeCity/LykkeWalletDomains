using System.Threading.Tasks;

namespace Core.Accounts.Recovery
{
    public interface IPrivateKeyOwnershipMsgRepository
    {
        Task<string> GenerateMsgForEmail(string email);
        Task<string> GetMsgForEmail(string email);
        Task RemoveMsg(string email);
    }
}
