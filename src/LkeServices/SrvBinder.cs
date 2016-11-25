using System;
using System.Linq;
using System.Net;
using Autofac;
using Common;
using Common.HttpRemoteRequests;
using Common.IocContainer;
using Common.Log;
using Core;
using Core.Accounts;
using Core.Assets;
using Core.Bitcoin;
using Core.BitCoin;
using Core.BitCoin.Ninja;
using Core.Clients;
using Core.Ethereum;
using Core.Exchange;
using Core.Images;
using Core.Kyc;
using Core.LykkeNews;
using Core.LykkeServiceApi;
using Core.Messages;
using Core.Messages.Email;
using Core.Messages.Email.ContentGenerator;
using Core.Messages.Email.Sender;
using Core.Messages.Sms;
using Core.Notifications;
using Core.PaymentSystems.CreditVoucher;
using Core.QrCode;
using Core.Security;
using Core.Settings;
using Core.StaticContent;
using LkeServices.Account;
using LkeServices.Assets;
using LkeServices.Assets.AssetGroups;
using LkeServices.Bitcoin;
using LkeServices.Bitcoin.BlockchainCommands;
using LkeServices.Clients;
using LkeServices.Ethereum;
using LkeServices.EventLogs;
using LkeServices.Exchange;
using LkeServices.Feed;
using LkeServices.Images;
using LkeServices.Kyc;
using LkeServices.LykkeNews;
using LkeServices.LykkeServiceApi;
using LkeServices.MeConnector;
using LkeServices.Messages;
using LkeServices.Messages.Email;
using LkeServices.Messages.Sms;
using LkeServices.Notifications;
using LkeServices.PaymentSystems;
using LkeServices.PaymentSystems.CreditVoucher;
using LkeServices.PaymentSystems.PaymentOkNotificators;
using LkeServices.Pdf;
using LkeServices.QrCode;
using LkeServices.Security;
using LkeServices.Settings;
using LkeServices.StaticContent;
using TcpSockets;

namespace LkeServices
{
    public static class SrvBinder
    {
        public static void BindTransferRequestServices(ContainerBuilder ioc)
        {
            ioc.RegisterType<SrvCashTransfer>().SingleInstance();
        }

        public static void BindTraderPortalServices(ContainerBuilder ioc)
        {
            ioc.RegisterType<AssetPairsBestRateCache>().SingleInstance();
            ioc.RegisterType<SrvIpGeolocation>().As<ISrvIpGetLocation>().SingleInstance();
        }

        private static void BindOrderReporBuilders(this ContainerBuilder ioc)
        {
            ioc.RegisterType<SrvOrderBookBuilder>().SingleInstance();
        }

        private static void BindKycServices(this ContainerBuilder ioc)
        {
            ioc.RegisterType<SrvKycManager>().SingleInstance();
        }

        public static void BindLykkeWalletApiServices(this ContainerBuilder ioc,
            string notificationsHubConnectionString, string notificationsHubName)
        {
            ioc.RegisterType<SrvIpGeolocation>().As<ISrvIpGetLocation>().SingleInstance();
            ioc.RegisterType<CreditVouchersSecurity>().As<ICreditVouchersSecurity>().SingleInstance();

            ioc.RegisterType<APIBankCardsCreditVouchers>().As<IApiBankCards>();

            ioc.RegisterType<SrvBlockchainHelper>().As<ISrvBlockchainHelper>().SingleInstance();
            ioc.RegisterType<HttpRequestClient>();

            ioc.BindCachedDicts();

            ioc.RegisterType<SrvBitcoinCommandProducer>().SingleInstance();
            ioc.RegisterType<SrvBlockchainValidations>().SingleInstance();

            ioc.RegisterType<QueueSmsRequestProducer>().As<ISmsRequestProducer>().SingleInstance();

            ioc.RegisterType<ImageBuilder>().As<IImageBuilder>().SingleInstance();

            ioc.RegisterType<SrvAssetsHelper>().SingleInstance();

            ioc.BindKycServices();

            ioc.RegisterType<StaticContentManager>().As<IStaticContentManager>().SingleInstance();
            ioc.RegisterType<SrvSecurityHelper>().As<ISrvSecurityHelper>().SingleInstance();
            ioc.RegisterType<LykkeNewsService>().As<ILykkeNewsService>().SingleInstance();
            ioc.RegisterType<SrvEthereumHelper>().As<ISrvEthereumHelper>().SingleInstance();

            ioc.RegisterType<SrvIcoLkkSoldCounter>().SingleInstance();

            ioc.RegisterType<SrvAppNotifications>().As<IAppNotifications>();

            ioc.Register<IAppNotifications>(x => new SrvAppNotifications(notificationsHubConnectionString, notificationsHubName)).SingleInstance();

            ioc.RegisterType<SrvKycForAsset>().As<ISrvKycForAsset>().SingleInstance();
            ioc.RegisterType<SrvSlackNotifications>().SingleInstance();
        }

        public static void BindPaymentSystemsServices(this ContainerBuilder ioc, string ninjaUrl)
        {
            ioc.RegisterType<CreditVouchersSecurity>().As<ICreditVouchersSecurity>().SingleInstance();
            ioc.BindCachedDicts();
            ioc.RegisterType<SrvBlockchainHelper>().As<ISrvBlockchainHelper>().SingleInstance();
            ioc.Register<ISrvBlockchainReader>(x => new SrvNinjaBlockChainReader(ninjaUrl)).SingleInstance();
            ioc.RegisterType<SrvBitcoinCommandProducer>().SingleInstance();
            ioc.RegisterType<HttpRequestClient>();
        }

        public static void BindBackOfficeServices(this ContainerBuilder ioc,
            string notificationsHubConnectionString, string notificationsHubName)
        {
            ioc.RegisterType<SrvClientFinder>().SingleInstance();
            ioc.RegisterType<SrvBlockchainHelper>().As<ISrvBlockchainHelper>().SingleInstance();
            ioc.RegisterType<HttpRequestClient>();
            ioc.BindCachedDicts();
            ioc.RegisterType<SrvBitcoinCommandProducer>().SingleInstance();
            ioc.RegisterType<SrvIpGeolocation>().As<ISrvIpGetLocation>().SingleInstance();
            ioc.RegisterType<SrvFailedTransactionsManager>().SingleInstance();
            ioc.BindKycServices();
            ioc.BindOrderReporBuilders();
            ioc.Register<IAppNotifications>(x => new SrvAppNotifications(notificationsHubConnectionString, notificationsHubName));

            ioc.RegisterType<QueueSmsRequestProducer>().As<ISmsRequestProducer>().SingleInstance();
            ioc.RegisterType<SrvAssetsHelper>().SingleInstance();
            ioc.RegisterType<SrvSecurityHelper>().As<ISrvSecurityHelper>().SingleInstance();
            ioc.RegisterType<SrvPaymentProcessor>().SingleInstance();
            ioc.RegisterType<SrvIcoLkkSoldCounter>().SingleInstance();
        }

        public static void BindLykkeServicesApi(this ContainerBuilder ioc, LykkeServiceApiSettings serviceApiSettings)
        {
            ioc.Register<ILykkeServiceApiConnector>(x => new LykkeServiceApiConnector(serviceApiSettings));

            ioc.RegisterType<IssuerApiService>().As<IIssuerApiService>().SingleInstance();
            ioc.RegisterType<AssetApiService>().As<IAssetApiService>().SingleInstance();
            ioc.RegisterType<AssetPairApiService>().As<IAssetPairApiService>().SingleInstance();
        }

        public static void BindTransactionsHandlerServices(this ContainerBuilder ioc, SrvConditionsManagerSettings settings)
        {
            ioc.BindCachedDicts();
            ioc.RegisterInstance(settings);
            ioc.RegisterType<SrvCommandRegistrator>().SingleInstance();
            ioc.RegisterType<SrvBlockchainHelper>().As<ISrvBlockchainHelper>().SingleInstance();
            ioc.RegisterType<HttpRequestClient>().SingleInstance();
            ioc.RegisterType<SrvBitcoinCommandProducer>().SingleInstance();
            ioc.RegisterType<SrvCommandsRunner>().SingleInstance();
            ioc.RegisterType<SrvConditionsManager>().SingleInstance();
        }


        public static void BindMessagesServices(this ContainerBuilder container, bool isDevEnv, bool useRemoteTemplates, ILog log)
        {
            container.RegisterType<SrvPdfGenerator>().SingleInstance();
            container.RegisterType<TemplateGenerator>().SingleInstance();
            if (useRemoteTemplates)
            {
                container.RegisterType<HttpRequestClient>();
                container.RegisterType<RemoteTemplateGenerator>().As<ITemplateGenerator>().SingleInstance();
            }
            else
            {
                container.RegisterType<TemplateGenerator>().As<ITemplateGenerator>().SingleInstance();
            }

            container.RegisterType<EmailGenerator>().As<IEmailGenerator>().SingleInstance();
            container.RegisterType<SmsTextGenerator>().As<ISmsTextGenerator>().SingleInstance();
            container.RegisterType<QrCodeGenerator>().As<IQrCodeGenerator>().SingleInstance();

            if (isDevEnv)
            {
                container.RegisterType<SmsMockSender>().As<ISmsSender>().SingleInstance();
                container.RegisterType<AlternativeSmsMockSender>().As<IAlternativeSmsSender>().SingleInstance();
            }
            else
            {
                container.RegisterType<NexmoSmsSender>().As<ISmsSender>().SingleInstance();
                container.RegisterType<TwilioSmsSender>().As<IAlternativeSmsSender>().SingleInstance();
            }

            container.RegisterType<SrvSlackNotifications>().SingleInstance();
        }

        public static void RegisterWalletBackendResponseHandlerServices(this ContainerBuilder ioc, string ninjaUrl, string notificationsHubConnStr,
            string notificationsHubName)
        {
            ioc.RegisterType<SrvBlockchainHelper>().As<ISrvBlockchainHelper>().SingleInstance();
            ioc.RegisterType<HttpRequestClient>();
            ioc.Register<ISrvBlockchainReader>(x => new SrvNinjaBlockChainReader(ninjaUrl)).SingleInstance();
            ioc.RegisterType<SrvFailedTransactionsManager>().SingleInstance();
            ioc.RegisterType<SrvBitcoinCommandProducer>().SingleInstance();
            ioc.Register<IAppNotifications>(x => new SrvAppNotifications(notificationsHubConnStr, notificationsHubName));
        }

        public static void RegisterTxDetectorServices(this ContainerBuilder ioc, string ninjaUrl, string notificationsHubConnStr,
            string notificationsHubName)
        {
            ioc.RegisterType<SrvBlockchainHelper>().As<ISrvBlockchainHelper>().SingleInstance();
            ioc.RegisterType<HttpRequestClient>();
            ioc.Register<ISrvBlockchainReader>(x => new SrvNinjaBlockChainReader(ninjaUrl)).SingleInstance();
            ioc.RegisterType<SrvBitcoinCommandProducer>().SingleInstance();
            ioc.Register<IAppNotifications>(x => new SrvAppNotifications(notificationsHubConnStr, notificationsHubName));
        }

        public static void RegisterBackgroundWorkerServices(this ContainerBuilder ioc)
        {
            ioc.RegisterType<SrvEthereumHelper>().As<ISrvEthereumHelper>().SingleInstance();
            ioc.RegisterType<SrvIpGeolocation>().As<ISrvIpGetLocation>().SingleInstance();
            ioc.RegisterType<SrvReferralCodeFinder>().As<ISrvReferralCodeFinder>().SingleInstance();
        }       

        public static void RegisterEthereumQueueHandlerServices(this ContainerBuilder ioc, string ninjaUrl)
        {
            ioc.RegisterType<SrvBlockchainHelper>().As<ISrvBlockchainHelper>().SingleInstance();
            ioc.RegisterType<HttpRequestClient>();
            ioc.Register<ISrvBlockchainReader>(x => new SrvNinjaBlockChainReader(ninjaUrl)).SingleInstance();
            ioc.RegisterType<SrvBitcoinCommandProducer>().SingleInstance();
        }

        public static void RegisterBtcConversionWalletDetectorServices(this ContainerBuilder ioc, string ninjaUrl, string notificationsHubConnStr,
            string notificationsHubName)
        {
            ioc.RegisterType<SrvBlockchainHelper>().As<ISrvBlockchainHelper>().SingleInstance();
            ioc.RegisterType<HttpRequestClient>();
            
            ioc.Register<ISrvBlockchainReader>(x => new SrvNinjaBlockChainReader(ninjaUrl)).SingleInstance();
            ioc.RegisterType<SrvBitcoinCommandProducer>().SingleInstance();
            ioc.Register<IAppNotifications>(x => new SrvAppNotifications(notificationsHubConnStr, notificationsHubName)).SingleInstance();
        }

        public static void RegisterServicesMonitoringJobServices(this ContainerBuilder ioc)
        {
            ioc.BindCachedDicts();
            ioc.RegisterType<SrvSlackNotifications>().SingleInstance();
        }

        public static void BindCachedDicts(this ContainerBuilder ioc)
        {
            ioc.Register(x => {
                var ctx = x.Resolve<IComponentContext>();
                return new CachedDataDictionary<string, IAsset>(
                    async () => (await ctx.Resolve<IAssetsRepository>().GetAssetsAsync()).ToDictionary(itm => itm.Id));
            }).SingleInstance();

            ioc.Register(x => {
                var ctx = x.Resolve<IComponentContext>();
                return new CachedDataDictionary<string, IAssetPair>(
                    async () => (await ctx.Resolve<IAssetPairsRepository>().GetAllAsync()).ToDictionary(itm => itm.Id));
            }).SingleInstance();
        }

        #region MatchingEngine

        public static void BindMatchingEngineChannel(this ContainerBuilder container, IPEndPoint ipEndPoint)
        {

            var socketLog = new SocketLogDynamic(i => { },
                str => Console.WriteLine(DateTime.UtcNow.ToIsoDateTime() + ": " + str)
                );

            var tcpClient = new TcpClientMatchingEngineConnector(ipEndPoint, socketLog);
            container.RegisterInstance<IMatchingEngineConnector>(tcpClient);
            tcpClient.Start();

        }

        public static void BindMatchingEngineServices(this ContainerBuilder ioc)
        {
            ioc.BindCachedDicts();
        }

        public static void StartMatchingEngineServices(this ContainerBuilder ioc)
        {

        }

        #endregion





        public static void RegisterAllServices(this ContainerBuilder ioc)
        {
            // Payment services
            ioc.RegisterType<SrvRateCalculator>().As<ISrvRateCalculator>().SingleInstance();
            ioc.RegisterType<PaymentOkEmailSender>().As<IPaymentOkNotification>().SingleInstance();

            // Email Services
            ioc.RegisterType<EmailSender>().As<IEmailSender>().SingleInstance();
            ioc.RegisterType<SrvEmailsFacade>().As<ISrvEmailsFacade>().SingleInstance();

        }

    }
}
