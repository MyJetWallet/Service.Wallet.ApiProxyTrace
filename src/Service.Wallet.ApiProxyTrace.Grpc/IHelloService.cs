using System.ServiceModel;
using System.Threading.Tasks;
using Service.Wallet.ApiProxyTrace.Grpc.Models;

namespace Service.Wallet.ApiProxyTrace.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}