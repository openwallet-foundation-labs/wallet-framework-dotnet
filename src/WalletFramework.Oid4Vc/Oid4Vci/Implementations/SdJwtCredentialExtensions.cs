using System.Drawing;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt;
using WalletFramework.SdJwtLib.Models;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Models.Credential;

namespace WalletFramework.Oid4Vc.Oid4Vci.Implementations;

public static class SdJwtCredentialExtensions
{
    public static SdJwtCredential ToCredential(
        this SdJwtDoc sdJwtDoc,
        SdJwtConfiguration configuration,
        KeyId keyId,
        CredentialSetId credentialSetId,
        bool isOneTimeUse)
    {
        var displaysOption =
            from displays in configuration.CredentialConfiguration.Display
            select displays.Select(credentialDisplay =>
            {
                var backgroundColor = credentialDisplay.BackgroundColor.ToNullable() ?? Color.White;
                var textColor = credentialDisplay.TextColor.ToNullable() ?? Color.Black;

                return new SdJwtDisplay
                {
                    Logo = new SdJwtDisplay.SdJwtLogo
                    {
                        AltText = credentialDisplay.Logo.ToNullable()?.AltText.ToNullable(),
                        Uri = credentialDisplay.Logo.ToNullable()?.Uri!
                    },
                    Name = credentialDisplay.Name.ToNullable(),
                    BackgroundColor = backgroundColor,
                    Locale = credentialDisplay.Locale.ToNullable(),
                    TextColor = textColor
                };
            }).ToList();

        var expiresAt = sdJwtDoc.UnsecuredPayload.SelectToken("exp")?.Value<long>() is { } exp
            ? Option<DateTime>.Some(DateTimeOffset.FromUnixTimeSeconds(exp).DateTime)
            : Option<DateTime>.None;

        var credential = new SdJwtCredential(
            sdJwtDoc,
            CredentialId.CreateCredentialId(),
            credentialSetId,
            displaysOption,
            keyId,
            CredentialState.Active,
            isOneTimeUse,
            expiresAt);

        return credential;
    }
}
