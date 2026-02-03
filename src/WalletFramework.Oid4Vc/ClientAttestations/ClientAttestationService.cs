using Microsoft.Extensions.Options;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.ClientAttestations.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.WalletAttestations;
using WalletFramework.Oid4Vc.WalletAttestations.Abstractions;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.ClientAttestations;

public class ClientAttestationService(
    IOptions<ClientOptions> clientOptions,
    ISdJwtSigner sdJwtSigner,
    IWalletAttestationService walletAttestationService) : IClientAttestationService
{
    public async Task<ClientAttestation> GetClientAttestation(AuthorizationServerMetadata authorizationServerMetadata)
    {
        var walletAttestation = (await walletAttestationService.RequestWalletAttestation()).UnwrapOrThrow();
        
        var walletAttestationPopJwt = await GeneratePoPJwt(
            walletAttestation.KeyId,
            authorizationServerMetadata.Issuer,
            clientOptions.Value.ClientId);

        return ClientAttestation.Create(walletAttestation, walletAttestationPopJwt);
    }

    private async Task<WalletAttestationPopJwt> GeneratePoPJwt(
        KeyId keyId,
        string audience,
        string issuer)
    {
        var header = new Dictionary<string, object>
        {
            { "alg", "ES256" },
            { "typ", "oauth-client-attestation-pop+jwt" }
        };

        var payload = new Dictionary<string, object>
        {
            { "aud", audience },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "exp", DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds() },
            { "jti", Guid.NewGuid().ToString() },
            { "iss", issuer }
        };

        return WalletAttestationPopJwt.Create(await sdJwtSigner.CreateSignedJwt(header, payload, keyId));
    }
}
