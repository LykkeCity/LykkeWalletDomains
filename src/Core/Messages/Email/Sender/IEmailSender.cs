using System.Threading.Tasks;
using Core.Broadcast;
using Core.Messages.Email.ContentGenerator.MessagesData;

namespace Core.Messages.Email.Sender
{

    public interface IEmailSender
    {
        Task SendEmailAsync<T>(string emailAddress, T messageData) where T : IEmailMessageData;
        Task SendEmailBroadcastAsync<T>(BroadcastGroup broadcastGroup, T messageData) where T : IEmailMessageData;
    }


    public static class EmailSenderExt
    {

        public static Task BroadcastEmailAsync(this IEmailSender emailSender, BroadcastGroup broadcastGroup, string subject, string body)
        {
            var model = PlainTextBroadCastData.Create(subject, body);
            return emailSender.SendEmailBroadcastAsync(broadcastGroup, model);
        } 
    }

}
