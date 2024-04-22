using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MySettingsReader;
using Service.Wallet.ApiProxyTrace.Settings;

namespace Service.Wallet.ApiProxyTrace
{
    public class Program
    {
        public const string SettingsFileName = ".myjetwallet";

        public static SettingsModel Settings { get; private set; }

        public static ILoggerFactory LogFactory { get; private set; }

        public static Func<T> ReloadedSettings<T>(Func<SettingsModel, T> getter)
        {
            return () =>
            {
                var settings = SettingsReader.GetSettings<SettingsModel>(SettingsFileName);
                var value = getter.Invoke(settings);
                return value;
            };
        }

        private static string ReadEnvVariable(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine($"!!! Cannot read env variable: {name} !!!");
                throw new Exception($"Cannot read env variable: {name}.");
            }
            return value;
        }

        public static void Main(string[] args)
        {
            Console.Title = "MyJetWallet Service.Wallet.ApiProxyTrace";

            Console.WriteLine();
            Console.WriteLine("Env variables:");
            Console.WriteLine("  'PROXY-HOST' - route request to host");
            Console.WriteLine("  'TRACE-INDEX-PREFIX' - elk index prefix for trace");
            Console.WriteLine("  'LOG-INDEX-PREFIX' - elk index prefix for logs");
            Console.WriteLine("  'ELK-USER' - ELK user for auth");
            Console.WriteLine("  'ELK-PASSWORD' - ELK password for auth");
            Console.WriteLine("  'ELK-URL' - ELK api url address");
            Console.WriteLine();
            
            var settingsExist = false;
            try
            {
                Settings = SettingsReader.GetSettings<SettingsModel>(SettingsFileName);
                Settings.ProxyHost = "https://simple.app";
                settingsExist = !string.IsNullOrEmpty(Settings?.TraceIndexPrefix);
            }
            catch (Exception)
            {
            }

            if (!settingsExist)
            {
                var settings = new SettingsModel()
                {
                    SeqServiceUrl = string.Empty,
                    ZipkinUrl = string.Empty,
                    ProxyHost = ReadEnvVariable("PROXY-HOST"),
                    TraceIndexPrefix = ReadEnvVariable("TRACE-INDEX-PREFIX"),
                    ElkLogs = new LogElkSettings()
                    {
                        IndexPrefix = ReadEnvVariable("LOG-INDEX-PREFIX"),
                        User = ReadEnvVariable("ELK-USER"),
                        Password = ReadEnvVariable("ELK-PASSWORD"),
                        Urls = new Dictionary<string, string>()
                        {
                            {"1", ReadEnvVariable("ELK-URL")}
                        }
                    }
                };

                Settings = settings;
            }

            //Settings.TraceIndexPrefix = "proxy-trace";
            Console.WriteLine($"Trace index prefix: {Settings.TraceIndexPrefix}");
            
            using var loggerFactory = LogConfigurator.ConfigureElk_v2("MyJetWallet", Settings.SeqServiceUrl, Settings.ElkLogs);

            var logger = loggerFactory.CreateLogger<Program>();

            LogFactory = loggerFactory;

            try
            {
                logger.LogInformation("Application is being started");

                CreateHostBuilder(loggerFactory, args).Build().Run();

                logger.LogInformation("Application has been stopped");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Application has been terminated unexpectedly");
            }
        }

        public static IHostBuilder CreateHostBuilder(ILoggerFactory loggerFactory, string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var httpPort = Environment.GetEnvironmentVariable("HTTP_PORT") ?? "8080";

                    Console.WriteLine($"HTTP PORT: {httpPort}");

                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, int.Parse(httpPort), o => o.Protocols = HttpProtocols.Http1);
                    });

                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(loggerFactory);
                    services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                });
    }
}
