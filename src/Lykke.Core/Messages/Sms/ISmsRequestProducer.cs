using System.Threading.Tasks;

namespace Core.Messages.Sms
{
    public interface ISmsRequestProducer
    {
        Task SendSmsAsync<T>(string phoneNumber, T msgData, bool useAlternativeProvider);
    }
}
