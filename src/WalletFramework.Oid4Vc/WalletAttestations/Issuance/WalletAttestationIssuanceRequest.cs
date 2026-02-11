using System.Text.Json;

namespace WalletFramework.Oid4Vc.WalletAttestations.Issuance;

public record ConfirmationClaimJwk(
    string Kty,
    string Use,
    string Crv,
    string X,
    string Y,
    string Alg);

public record WalletAttestationRequest(
    WalletInstanceId WalletInstanceId,
    ConfirmationClaimJwk ConfirmationClaimJwk);

public static class WalletAttestationIssuanceRequestFun
{
    public static string ToJson(this WalletAttestationRequest request)
    {
        var jwk = new Dictionary<string, string>
        {
            { WalletAttestationJsonFields.JwkKty, request.ConfirmationClaimJwk.Kty },
            { WalletAttestationJsonFields.JwkUse, request.ConfirmationClaimJwk.Use },
            { WalletAttestationJsonFields.JwkCrv, request.ConfirmationClaimJwk.Crv },
            { WalletAttestationJsonFields.JwkX, request.ConfirmationClaimJwk.X },
            { WalletAttestationJsonFields.JwkY, request.ConfirmationClaimJwk.Y },
            { WalletAttestationJsonFields.JwkAlg, request.ConfirmationClaimJwk.Alg },
        };

        var dictionary = new Dictionary<string, object>
        {
            { WalletAttestationJsonFields.WalletInstanceId, request.WalletInstanceId.Value },
            { WalletAttestationJsonFields.ConfirmationClaimJwk, jwk },
        };

        return JsonSerializer.Serialize(dictionary);
    }
}
