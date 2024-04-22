using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using MyJetWallet.Sdk.GrpcSchema;
using MyJetWallet.Sdk.RestApiTrace;
using MyJetWallet.Sdk.Service;
using Service.Wallet.ApiProxyTrace.Modules;
using Service.Wallet.ApiProxyTrace.Services;

namespace Service.Wallet.ApiProxyTrace
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureJetWallet<ApplicationLifetimeManager>(Program.Settings.ZipkinUrl);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ApiTraceMiddleware.ContextHandlerCallback = (context, item) =>
            {
                
            };
                
            app.UseMiddleware<ApiTraceMiddleware>();
            app.UseMiddleware<ProxyMiddleware>();
            Console.WriteLine("API Trace is Enabled");
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.ConfigureJetWallet();
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<ServiceModule>();
        }
    }
}
