using LanguageExt;
using WalletFramework.Oid4Vc.ClientAttestation;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Abstractions;

public interface IDPopHttpClient
{
    internal Task<DPopHttpResponse> Post(
        Uri requestUri,
        DPopConfig config,
        Option<CombinedWalletAttestation> combinedWalletAttestation,
        Func<HttpContent> getContent);
}
