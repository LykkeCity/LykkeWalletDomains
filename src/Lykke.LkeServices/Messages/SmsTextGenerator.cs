using System;
using System.IO;
using System.Threading.Tasks;
using Core.Messages;
using Core.Messages.Sms;
using Core.Sms;
using Core.Sms.MessagesData;
using LkeServices.Messages.Settings;
using LkeServices.Messages.SmsTemplates.ViewModels;

namespace LkeServices.Messages
{
    public class SmsTextGenerator : ISmsTextGenerator
    {
        private readonly TemplateGenerator _templateGenerator;

        public SmsTextGenerator(TemplateGenerator templateGenerator)
        {
            _templateGenerator = templateGenerator;
        }

        public async Task<string> GenerateConfirmSmsText(SmsConfirmationData confirmationDataData)
        {
            var templateVm = new SmsConfirmationTemplate
            {
                ConfirmationCode = confirmationDataData.ConfirmationCode
            };

            return await _templateGenerator.GenerateAsync("SmsConfirmation", templateVm, TemplateType.Sms);
        }
    }
}
