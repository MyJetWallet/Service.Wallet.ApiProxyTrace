using Autofac;
using Service.Wallet.ApiProxyTrace.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.Wallet.ApiProxyTrace.Client
{
    public static class AutofacHelper
    {
        public static void RegisterWallet.ApiProxyTraceClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new Wallet.ApiProxyTraceClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetHelloService()).As<IHelloService>().SingleInstance();
        }
    }
}
