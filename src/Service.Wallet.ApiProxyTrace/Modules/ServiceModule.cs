using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.RestApiTrace;

namespace Service.Wallet.ApiProxyTrace.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ApiTraceManager>()
                .WithParameter("elkSettings", Program.Settings.ElkLogs)
                .WithParameter("elkIndexPrefix", Program.Settings.TraceIndexPrefix)
                .WithParameter("logger", Program.LogFactory.CreateLogger("ApiTraceManager"))
                
            
                .As<IApiTraceManager>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}