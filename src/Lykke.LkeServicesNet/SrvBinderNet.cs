using System;
using System.Net;
using Autofac;
using Common.IocContainer;
using Common.Log;
using Core.Broadcast;
using Core.Messages.Email;
using Core.Messages.Email.Sender;
using LkeServicesNet.Messages.Email;
using LkeServicesNet.Messages.Settings;
using MailKit.Net.Smtp;
using MimeKit;

namespace LkeServicesNet
{
	public class MessageSettings
	{
		public EmailSettings EmailSettings { get; set; }

		public bool IsDevEnv { get; set; }
	}


	public static class SrvBinderNet
	{
		static bool useBoth = true;

		public static void BindMessageServices(this ContainerBuilder container, MessageSettings settings, ILog log)
		{
			Func<SmtpClient> clientFactory = () =>
			{
				var client = new SmtpClient()
				{
					Timeout = 10000,
				};
				client.Connect(settings.EmailSettings.SmtpHost, settings.EmailSettings.SmtpPort);
				client.Authenticate(new NetworkCredential(settings.EmailSettings.SmtpLogin, settings.EmailSettings.SmtpPwd));
				return client;
			};			

			var from = new MailboxAddress(settings.EmailSettings.EmailFromDisplayName, settings.EmailSettings.EmailFrom);
			if (settings.IsDevEnv)
			{
				container.RegisterType<SmtpMailSenderMock>().SingleInstance();
				container.Register(x => new SmtpMailSender(log, clientFactory, from, x.Resolve<IBroadcastMailsRepository>()));

				if (useBoth)
					container.RegisterType<MockAndRealMailSender>().As<ISmtpEmailSender>().SingleInstance();
				else
					container.RegisterType<SmtpMailSenderMock>().As<ISmtpEmailSender>().SingleInstance();
			}
			else
				container.Register<ISmtpEmailSender>(x => new SmtpMailSender(log, clientFactory, from, x.Resolve<IBroadcastMailsRepository>()));

		}

	}
}
