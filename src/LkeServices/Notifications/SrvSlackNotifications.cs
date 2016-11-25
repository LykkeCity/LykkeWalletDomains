using System.Text;
using System.Threading.Tasks;
using Common;
using Common.HttpRemoteRequests;
using Core.Settings;

namespace LkeServices.Notifications
{
    public class SrvSlackNotifications
    {
        private readonly SlackIntegrationSettings _slackIntegrationSettings;
        public SrvSlackNotifications(BaseSettings baseSettings)
        {
            _slackIntegrationSettings = baseSettings.SlackIntegration;
        }

        public async Task SendNotification(string type, string message, string sender = null)
        {
            var webHookUrl = _slackIntegrationSettings.GetChannelWebHook(type);
            if (webHookUrl != null)
            {
                var text = new StringBuilder();

                if (!string.IsNullOrEmpty(_slackIntegrationSettings.Env))
                    text.AppendLine($"Environment: {_slackIntegrationSettings.Env}");

                text.AppendLine(sender != null ? $"{sender} : {message}" : message);

                await
                    new HttpRequestClient().Request(new { text = text.ToString() }.ToJson(),
                        webHookUrl);
            }
        }
    }
}
