using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Wallet.ApiProxyTrace.Settings
{
    public class SettingsModel
    {
        [YamlProperty("WalletApiProxyTrace.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("WalletApiProxyTrace.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("WalletApiProxyTrace.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
        
        [YamlProperty("WalletApiProxyTrace.ProxyHost")]
        public string ProxyHost { get; set; }
        
        [YamlProperty("WalletApiProxyTrace.TraceIndexPrefix")]
        public string TraceIndexPrefix { get; set; }
    }
}
