using System.Drawing;
using Microsoft.IdentityModel.Tokens;
using SD_JWT.Models;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Models;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;
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
            .SelectMany(claimMetadata => 
            {
                var claimMetadatas = new Dictionary<string, ClaimMetadata> { { claimMetadata.Key, claimMetadata.Value } };

                if (!claimMetadata.Value.NestedClaims.IsNullOrEmpty())
                {
                    foreach (var nested in claimMetadata.Value.NestedClaims!)
                    {
                        claimMetadatas.Add(claimMetadata.Key + "." + nested.Key, nested.Value?.ToObject<ClaimMetadata>()!);
                    }
                }
                
                return claimMetadatas;
            })
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
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
        
        var record = new SdJwtRecord(
            sdJwtDoc,
            claims!,
            display!,
            keyId);

        return record;
    }
}
