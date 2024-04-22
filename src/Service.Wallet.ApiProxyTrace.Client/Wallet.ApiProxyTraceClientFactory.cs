using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.Wallet.ApiProxyTrace.Grpc;

namespace Service.Wallet.ApiProxyTrace.Client
{
    [UsedImplicitly]
    public class Wallet.ApiProxyTraceClientFactory: MyGrpcClientFactory
    {
        public Wallet.ApiProxyTraceClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IHelloService GetHelloService() => CreateGrpcService<IHelloService>();
    }
}
