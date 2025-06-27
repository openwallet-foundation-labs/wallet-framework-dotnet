
using OneOf;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.ClientAttestation;

public class ClientAttestationService : IClientAttestationService
{
    private readonly ISdJwtSigner _sdJwtSigner;

    public ClientAttestationService(
        ISdJwtSigner sdJwtSigner)
    {
        _sdJwtSigner = sdJwtSigner;
    }

    public async Task<CombinedWalletAttestation> GetCombinedWalletAttestationAsync(ClientAttestationDetails clientAttestationDetails, OneOf<AuthorizationServerMetadata, AuthorizationRequest> audienceSource)
    {
        var walletAttestationPopJwt = await audienceSource.Match(
            async authorizationServerMetadata => await GenerateWalletAttestationProofOfPossessionJwt(clientAttestationDetails.AssociatedKeyId, authorizationServerMetadata.Issuer, clientAttestationDetails.ClientAttestationPopDetails.ClientId, null),
            async authorizationRequest => await GenerateWalletAttestationProofOfPossessionJwt(clientAttestationDetails.AssociatedKeyId, authorizationRequest.ClientId, clientAttestationDetails.ClientAttestationPopDetails.ClientId, null)
            );
         
        return CombinedWalletAttestation.Create(clientAttestationDetails.WalletInstanceAttestationJwt, walletAttestationPopJwt);
    }
    
    private async Task<WalletInstanceAttestationPopJwt> GenerateWalletAttestationProofOfPossessionJwt(KeyId keyId, string audience, string issuer, string? nonce)
    {
        var header = new Dictionary<string, object>
        {
            { "alg", "ES256" },
            { "typ", "oauth-client-attestation+jwt" }
        };

        var payload = new Dictionary<string, object>
        {
            { "aud", audience },
            { "nonce", nonce },
            { "iat" , DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "exp" , DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds() },
            { "jti", Guid.NewGuid().ToString() },
            { "iss", issuer }
        };

        return WalletInstanceAttestationPopJwt.Create(await _sdJwtSigner.CreateSignedJwt(header, payload, keyId));
    }
}
