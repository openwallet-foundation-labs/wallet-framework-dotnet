using System.Text.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json.Errors;

namespace WalletFramework.Oid4Vc.WalletAttestations;

public record WalletAttestation(
    Guid Id,
    KeyId KeyId,
    JsonWebToken WalletAttestationJwt,
    WalletInstanceId WalletInstanceId);

public static class WalletAttestationFun
{
    public static Validation<WalletAttestation> FromJson(string json, KeyId keyId, WalletInstanceId walletInstanceId)
    {
        try
        {
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
            var walletAttestationJwt = new JsonWebToken(dictionary[WalletAttestationJsonFields.WalletAttestationJwt]);

            return new WalletAttestation(Guid.NewGuid(), keyId, walletAttestationJwt, walletInstanceId);
        }
        catch (Exception e)
        {
            return new InvalidJsonError(json, e);
        }
    }
}
