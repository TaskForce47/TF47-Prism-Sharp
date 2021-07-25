using System;
using System.Buffers.Text;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using TF47_Prism_Sharp.Models;
using TF47_Prism_Sharp.Services;

namespace TF47_Prism_Sharp
{
    public static class Configuration
    {
        public static string ApiKey = String.Empty;
        public static string BaseUrl = String.Empty;
        public static string MissionId = String.Empty;
        public static int SessionId = -1;
    }

    public static class Application
    {
        public static ServiceProvider ServiceProvider;

        public static void BuildApplication()
        {
            var services = new ServiceCollection();
            services.AddHttpClient("api", c =>
                {
                    c.BaseAddress = new Uri(Configuration.BaseUrl);
                    c.DefaultRequestHeaders.Add("Content-Type", "application/json");
                    c.DefaultRequestHeaders.Add("TF47AuthKey", Configuration.ApiKey);
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy());
            services.AddScoped<ApiClient>();
            services.AddSingleton<MediatorService>();
            ServiceProvider = services.BuildServiceProvider();
        }

        public static void ReadConfiguration()
        {
            var settingsFile = Path.Combine(Environment.CurrentDirectory, "settings.json");
            if (!File.Exists(settingsFile))
                throw new FileNotFoundException($"settings.json could not be found. Path: {settingsFile}");
            var json = File.ReadAllText(settingsFile, Encoding.UTF8);
            var settings = JsonSerializer.Deserialize<SettingsJson>(json);
            Configuration.ApiKey = settings.ApiKey;
            Configuration.BaseUrl = settings.Hostname;
        }
        
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt)));
        }
    }
}