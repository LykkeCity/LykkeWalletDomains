using System.Threading.Tasks;
using Core.Broadcast;
using Core.Messages.Email;
using Core.Messages.Email.Sender;

namespace LkeServicesNet.Messages.Email
{
    public class MockAndRealMailSender : ISmtpEmailSender
    {
        private readonly SmtpMailSenderMock _smtpSenderMock;
        private readonly SmtpMailSender _smtpMailSender;

        public MockAndRealMailSender(SmtpMailSenderMock smtpSenderMock,
            SmtpMailSender smtpMailSender)
        {
            _smtpSenderMock = smtpSenderMock;
            _smtpMailSender = smtpMailSender;
        }

        public async Task SendEmailAsync(string emailAddress, EmailMessage message, string sender = null)
        {
            await _smtpSenderMock.SendEmailAsync(emailAddress, message, sender);
            await _smtpMailSender.SendEmailAsync(emailAddress, message, sender);
        }

        public async Task SendBroadcastAsync(BroadcastGroup broadcastGroup, EmailMessage message)
        {
            await _smtpSenderMock.SendBroadcastAsync(broadcastGroup, message);
            await _smtpMailSender.SendBroadcastAsync(broadcastGroup, message);
        }
    }
}
