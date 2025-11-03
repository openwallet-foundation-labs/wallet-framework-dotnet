using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.SdJwtLib.Models;
using WalletFramework.SdJwtVc;

namespace WalletFramework.Oid4Vc.Oid4Vci.Implementations;

public static class SdJwtCredentialExtensions
{
    public static SdJwtCredential ToCredential(
        this SdJwtDoc sdJwtDoc,
        Option<KeyId> keyId,
        CredentialSetId credentialSetId,
        bool isOneTimeUse)
    {
        var expiresAt = sdJwtDoc.UnsecuredPayload.SelectToken("exp")?.Value<long>() is { } exp
            ? Option<DateTime>.Some(DateTimeOffset.FromUnixTimeSeconds(exp).DateTime)
            : Option<DateTime>.None;

        var credential = new SdJwtCredential(
            sdJwtDoc,
            CredentialId.CreateCredentialId(),
            credentialSetId,
            keyId,
            CredentialState.Active,
            isOneTimeUse,
            expiresAt);

        return credential;
    }
}
