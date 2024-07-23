using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Abstractions;

public interface IDPopHttpClient
{
    internal Task<DPopHttpResponse> Post(
        Uri requestUri,
        DPopConfig config,
        Func<HttpContent> getContent);
}
