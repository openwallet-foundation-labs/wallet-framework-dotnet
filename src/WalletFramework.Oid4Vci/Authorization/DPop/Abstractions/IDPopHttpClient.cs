using LanguageExt;
using WalletFramework.Oid4Vci.Authorization.DPop.Models;
using WalletFramework.WalletAttestations;

namespace WalletFramework.Oid4Vci.Authorization.DPop.Abstractions;

public interface IDPopHttpClient
{
    internal Task<DPopHttpResponse> Post(
        DPopConfig config,
        Func<HttpContent> getContent,
        Option<ClientAttestation> clientAttestation,
        Uri requestUri);
}
