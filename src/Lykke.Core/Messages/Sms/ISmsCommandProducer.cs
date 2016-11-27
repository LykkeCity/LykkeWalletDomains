using System.Threading.Tasks;

namespace Core.Messages.Sms
{
    public interface ISmsCommandProducer
    {
        Task ProduceSendSmsCommand<T>(string phoneNumber, T msgData, bool useAlternativeProvider);
    }
}
