namespace ServiceBus.Management.Infrastructure.OWIN
{
    using System;
    using Microsoft.Owin.Hosting;
    using NServiceBus;
    using NServiceBus.Logging;
    using Settings;

    public class OwinRunner : IWantToRunWhenBusStartsAndStops
    {
        public void Start()
        {
            var options = new StartOptions(Settings.ApiUrl);
            options.Urls.Add(Settings.ApiUrl.Replace("/api", ""));
            webApp = WebApp.Start<Startup>(options);
            Logger.InfoFormat("Api is now accepting requests on {0}", Settings.ApiUrl);
        }

        public void Stop()
        {
            webApp.Dispose();
            Logger.InfoFormat("Api is now stopped");
        }

        static readonly ILog Logger = LogManager.GetLogger(typeof(OwinRunner));
        IDisposable webApp;
    }
}