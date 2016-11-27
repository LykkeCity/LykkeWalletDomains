using System.Threading.Tasks;
using Core.Broadcast;

namespace Core.Messages.Email.Sender
{
    public interface ISmtpEmailSender
    {
        Task SendEmailAsync(string emailAddress, EmailMessage message, string sender = null);
        Task SendBroadcastAsync(BroadcastGroup broadcastGroup, EmailMessage message);
    }
}
