using System.Runtime.Serialization;
using Service.Wallet.ApiProxyTrace.Domain.Models;

namespace Service.Wallet.ApiProxyTrace.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}