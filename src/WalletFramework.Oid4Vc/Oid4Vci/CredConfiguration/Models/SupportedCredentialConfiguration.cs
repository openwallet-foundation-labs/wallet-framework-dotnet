using Newtonsoft.Json;
using OneOf;
using WalletFramework.Core.Json.Converters;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.SdJwt;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

[JsonConverter(typeof(OneOfJsonConverter<SupportedCredentialConfiguration, SdJwtConfiguration, MdocConfiguration>))]
public sealed class SupportedCredentialConfiguration : OneOfBase<SdJwtConfiguration, MdocConfiguration>
{
    public static implicit operator OneOf<SdJwtConfiguration, MdocConfiguration>(
        SupportedCredentialConfiguration supportedCredentialConfiguration) =>
        supportedCredentialConfiguration.Match(
            sdJwt => (OneOf<SdJwtConfiguration, MdocConfiguration>)sdJwt,
            mdoc => mdoc
        );
    
    public static implicit operator SupportedCredentialConfiguration(OneOf<SdJwtConfiguration, MdocConfiguration> input) =>
        new(input);
    
    public static implicit operator SupportedCredentialConfiguration(SdJwtConfiguration sdJwtConfiguration) =>
        new(sdJwtConfiguration);
    
    public static implicit operator SupportedCredentialConfiguration(MdocConfiguration mdocConfiguration) =>
        new(mdocConfiguration);
    
    private SupportedCredentialConfiguration(OneOf<SdJwtConfiguration, MdocConfiguration> input) : base(input)
    {
    }
}
