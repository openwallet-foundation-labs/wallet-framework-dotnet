using System.Drawing;
using SD_JWT.Models;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.SdJwtVc.Models.Records;

namespace WalletFramework.Oid4Vc.Oid4Vci.Implementations;

public static class SdJwtRecordExtensions
{
    public static SdJwtRecord ToRecord(
        this SdJwtDoc sdJwtDoc,
        SdJwtConfiguration configuration,
        IssuerMetadata issuerMetadata,
        KeyId keyId)
    {
        var claims = configuration
            .Claims?
            .Select(pair => (pair.Key, pair.Value))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        var display = configuration
            .CredentialConfiguration
            .Display
            .ToNullable()?
            .Select(credentialDisplay =>
            {
                var backgroundColor = credentialDisplay.BackgroundColor.ToNullable() ?? Color.White;
                var textColor = credentialDisplay.TextColor.ToNullable() ?? Color.Black;

                return new SdJwtDisplay
                {
                    Logo = new SdJwtDisplay.SdJwtLogo
                    {
                        AltText = credentialDisplay.Logo.ToNullable()?.AltText.ToNullable(),
                        Uri = credentialDisplay.Logo.ToNullable()?.Uri.ToNullable()!
                    },
                    Name = credentialDisplay.Name.ToNullable(),
                    BackgroundColor = backgroundColor,
                    Locale = credentialDisplay.Locale.ToNullable(),
                    TextColor = textColor
                };
            })
            .ToList();

        var issuerName = issuerMetadata
            .Display
            .ToNullable()?
            .ToDictionary(
                 issuerDisplay => issuerDisplay.Locale.ToNullable()?.ToString(),
                 issuerDisplay => issuerDisplay.Name.ToNullable()?.ToString());

        var record = new SdJwtRecord(
            sdJwtDoc,
            claims!,
            display!,
            issuerName!,
            keyId);

        return record;
    }
}
