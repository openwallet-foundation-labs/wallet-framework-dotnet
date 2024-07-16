using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.MdocLib;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.CryptographicCurve;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.CryptographicSuite;
using static WalletFramework.MdocLib.DocType;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.Policy;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.MdocConfiguration.MdocConfigurationJsonKeys;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

[JsonConverter(typeof(MdocConfigurationJsonConverter))]
public record MdocConfiguration
{
    public CredentialConfiguration CredentialConfiguration { get; }
    
    public DocType DocType { get; }
    
    // TODO: This is actually required, but BDR doesnt use it
    public Option<Policy> Policy { get; }
    
    public Option<List<CryptographicSuite>> CryptographicSuitesSupported { get; }
    
    public Option<List<CryptographicCurve>> CryptographicCurvesSupported { get; }
    
    public Option<ClaimsMetadata> Claims { get; }

    public Format Format => CredentialConfiguration.Format;

    private MdocConfiguration(
        CredentialConfiguration credentialConfiguration,
        DocType docType,
        Option<Policy> policy,
        Option<List<CryptographicSuite>> cryptographicSuitesSupported,
        Option<List<CryptographicCurve>> cryptographicCurvesSupported,
        Option<ClaimsMetadata> claims)
    {
        CredentialConfiguration = credentialConfiguration;
        DocType = docType;
        Policy = policy;
        CryptographicSuitesSupported = cryptographicSuitesSupported;
        CryptographicCurvesSupported = cryptographicCurvesSupported;
        Claims = claims;
    }

    private static MdocConfiguration Create(
        CredentialConfiguration credentialConfiguration,
        DocType docType,
        Option<Policy> policy,
        Option<List<CryptographicSuite>> cryptographicSuitesSupported,
        Option<List<CryptographicCurve>> cryptographicCurvesSupported,
        Option<ClaimsMetadata> claims) =>
        new(credentialConfiguration, docType, policy, cryptographicSuitesSupported, cryptographicCurvesSupported, claims);
    
    public static Validation<MdocConfiguration> ValidMdocConfiguration(JObject config)
    {
        var credentialConfiguration = CredentialConfiguration.ValidCredentialConfiguration(config);
        
        var docType = config.GetByKey(DocTypeJsonKey).OnSuccess(ValidDoctype);
        var policy = config.GetByKey(PolicyJsonKey).OnSuccess(ValidPolicy).ToOption();
        
        var cryptographicSuitesSupported = config
            .GetByKey(CryptographicSuitesSupportedJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.Select(ValidCryptographicSuite).TraverseAll(suite => suite))
            .OnSuccess(suites => suites.ToList())
            .ToOption();
        
        var cryptographicCurvesSupported = config
            .GetByKey(CryptographicCurvesSupportedJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.Select(ValidCryptographicCurve).TraverseAll(curve => curve))
            .OnSuccess(curves => curves.ToList())
            .ToOption();

        var claims = config
            .GetByKey(ClaimsJsonKey)
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(ClaimsMetadata.ValidClaimsMetadata)
            .ToOption();

        return ValidationFun.Valid(Create)
            .Apply(credentialConfiguration)
            .Apply(docType)
            .Apply(policy)
            .Apply(cryptographicSuitesSupported)
            .Apply(cryptographicCurvesSupported)
            .Apply(claims);
    }

    public static class MdocConfigurationJsonKeys
    {
        public const string DocTypeJsonKey = "doctype";
        public const string PolicyJsonKey = "policy";
        public const string CryptographicSuitesSupportedJsonKey = "cryptographic_suites_supported";
        public const string CryptographicCurvesSupportedJsonKey = "cryptographic_curves_supported";
        public const string ClaimsJsonKey = "claims";
    }
}

public class MdocConfigurationJsonConverter : JsonConverter<MdocConfiguration>
{
    public override bool CanRead => false;

    public override void WriteJson(JsonWriter writer, MdocConfiguration? mdocConfig, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        
        var credentialConfig = JObject.FromObject(mdocConfig!.CredentialConfiguration);
        foreach (var property in credentialConfig.Properties())
        {
            property.WriteTo(writer);
        }

        serializer.Converters.Add(new OptionJsonConverter<Policy>());
        serializer.Converters.Add(new OptionJsonConverter<List<CryptographicSuite>>());
        serializer.Converters.Add(new OptionJsonConverter<List<CryptographicCurve>>());
        serializer.Converters.Add(new OptionJsonConverter<ClaimsMetadata>());
        serializer.Converters.Add(new ValueTypeJsonConverter<CryptographicSuite>());

        writer.WritePropertyName(DocTypeJsonKey);
        serializer.Serialize(writer, mdocConfig.DocType);
        
        writer.WritePropertyName(PolicyJsonKey);
        serializer.Serialize(writer, mdocConfig.Policy);
        
        writer.WritePropertyName(CryptographicSuitesSupportedJsonKey);
        serializer.Serialize(writer, mdocConfig.CryptographicSuitesSupported);
        
        writer.WritePropertyName(CryptographicCurvesSupportedJsonKey);
        serializer.Serialize(writer, mdocConfig.CryptographicCurvesSupported);
        
        writer.WritePropertyName(ClaimsJsonKey);
        serializer.Serialize(writer, mdocConfig.Claims);

        writer.WriteEndObject(); 
    }

    public override MdocConfiguration ReadJson(JsonReader reader, Type objectType, MdocConfiguration? existingValue,
        bool hasExistingValue, JsonSerializer serializer) =>
        throw new NotImplementedException();
}
