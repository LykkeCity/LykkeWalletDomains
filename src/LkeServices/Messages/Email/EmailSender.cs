using System.Threading.Tasks;
using Core.Broadcast;
using Core.Messages.Email.ContentGenerator;
using Core.Messages.Email.ContentGenerator.MessagesData;
using Core.Messages.Email.Sender;

namespace LkeServices.Messages.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IEmailCommandProducer _emailCommandProducer;

        public EmailSender(IEmailCommandProducer emailCommandProducer)
        {
            _emailCommandProducer = emailCommandProducer;
        }

        public async Task SendEmailAsync<T>(string email, T msgData) where T: IEmailMessageData
        {
            await _emailCommandProducer.ProduceSendEmailCommand(email, msgData);
        }

        public async Task SendEmailBroadcastAsync<T>(BroadcastGroup broadcastGroup, T messageData) where T : IEmailMessageData
        {
            await _emailCommandProducer.ProduceSendEmailBroadcast(broadcastGroup, messageData);
        }
    }
}
