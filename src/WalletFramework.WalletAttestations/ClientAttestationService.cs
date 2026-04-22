using LanguageExt;
using Microsoft.Extensions.Options;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.WalletAttestations.Abstractions;

namespace WalletFramework.WalletAttestations;

public class ClientAttestationService(
    IOptions<ClientAttestationOptions> clientAttestationOptions,
    IAttestationSigner attestationSigner,
    IWalletAttestationService walletAttestationService) : IClientAttestationService
{
    public async Task<Option<ClientAttestation>> GetClientAttestation(ClientAttestationRequest request)
    {
        var walletAttestation = await walletAttestationService.RequestWalletAttestation();

        return await walletAttestation.MatchAsync<WalletAttestation, Option<ClientAttestation>>(
            async attestation =>
            {
                var walletAttestationPopJwt = await GeneratePoPJwt(
                    attestation.KeyId,
                    request.Audience,
                    clientAttestationOptions.Value.ClientId);

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
