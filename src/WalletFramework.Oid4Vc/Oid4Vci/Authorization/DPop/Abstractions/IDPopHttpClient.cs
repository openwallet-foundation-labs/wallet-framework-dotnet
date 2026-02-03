using LanguageExt;
using WalletFramework.Oid4Vc.ClientAttestations;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Abstractions;

public interface IDPopHttpClient
{
    internal Task<DPopHttpResponse> Post(
        DPopConfig config,
        Func<HttpContent> getContent,
        Option<ClientAttestation> combinedWalletAttestation,
        Uri requestUri);
}
