using LanguageExt;
using Microsoft.Extensions.Options;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Oid4Vc.ClientAttestations.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using WalletFramework.Oid4Vc.WalletAttestations;
using WalletFramework.Oid4Vc.WalletAttestations.Abstractions;

namespace WalletFramework.Oid4Vc.ClientAttestations;

public class ClientAttestationService(
    IOptions<ClientOptions> clientOptions,
    IAttestationSigner attestationSigner,
    IWalletAttestationService walletAttestationService) : IClientAttestationService
{
    public async Task<Option<ClientAttestation>> GetClientAttestation(AuthorizationServerMetadata authorizationServerMetadata)
    {
        var walletAttestation = await walletAttestationService.RequestWalletAttestation();

        return await walletAttestation.MatchAsync<WalletAttestation, Option<ClientAttestation>>(
            async attestation =>
            {
                var walletAttestationPopJwt = await GeneratePoPJwt(
                    attestation.KeyId,
                    authorizationServerMetadata.Issuer,
                    clientOptions.Value.ClientId);

                return ClientAttestation.Create(attestation, walletAttestationPopJwt);
            },
            () => Task.FromResult(Option<ClientAttestation>.None));
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
            { "exp", DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds() },
            { "jti", Guid.NewGuid().ToString() },
            { "iss", issuer }
        };

        return WalletAttestationPopJwt.Create(await attestationSigner.CreateSignedJwt(header, payload, keyId));
    }
}
