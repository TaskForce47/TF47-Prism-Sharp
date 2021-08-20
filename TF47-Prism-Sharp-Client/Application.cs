using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TF47_Prism_Sharp_Client.Services;
using TF47_Prism_Sharp_Dependencies.Services;

namespace TF47_Prism_Sharp_Client
{
    public static class Configuration
    {
        public static string ApiKey = string.Empty;
        public static string BaseUrl = string.Empty;
        public static string MissionId = string.Empty;
        public static int SessionId = -1;
        public static int ListeningPort = 6060;
        public static string Challenge = string.Empty;
    }

    public static class Application
    {
        public static ServiceProvider ServiceProvider;

        public static void BuildApplication()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new LoggerService("TF47-Prism-Sharp-Client", true));
            services.AddSingleton<SignalClient>();
            services.AddSingleton<MediatorService>();
            ServiceProvider = services.BuildServiceProvider();
        }
    }
}