using System.Threading.Tasks;
using Core.Sms.MessagesData;

namespace Core.Messages
{
    public interface ISmsTextGenerator
    {
        Task<string> GenerateConfirmSmsText(SmsConfirmationData confirmationDataData);
    }
}
