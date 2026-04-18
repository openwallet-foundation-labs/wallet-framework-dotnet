using WalletFramework.Oid4Vc.ClientAttestations;

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
    public static Dictionary<string, string> ToRequestEnvelope(this SignedWalletAttestationRequest signedRequest) =>
        new()
        {
            [WalletAttestationJsonFields.AssertionJwt] = signedRequest.SignedJwt.EncodedToken
        };
}
