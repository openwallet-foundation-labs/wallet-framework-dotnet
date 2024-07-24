using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.MdocLib;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.CryptographicCurve;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.CryptographicSuite;
using static WalletFramework.MdocLib.DocType;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.Policy;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.MdocConfigurationFun;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

public record MdocConfiguration
{
    public CredentialConfiguration CredentialConfiguration { get; }
    
    public DocType DocType { get; }
    
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
}

public static class MdocConfigurationFun
{
    public const string DocTypeJsonKey = "doctype";
    public const string PolicyJsonKey = "policy";
    public const string CryptographicSuitesSupportedJsonKey = "cryptographic_suites_supported";
    public const string CryptographicCurvesSupportedJsonKey = "cryptographic_curves_supported";
    public const string ClaimsJsonKey = "claims";

    public static JObject EncodeToJson(this MdocConfiguration mdocConfig)
    {
        var configJson = mdocConfig.CredentialConfiguration.EncodeToJson();

        configJson.Add(DocTypeJsonKey, mdocConfig.DocType.ToString());

        mdocConfig.Policy.IfSome(policy => 
            configJson.Add(PolicyJsonKey, policy.EncodeToJson())
        );
        
        mdocConfig.CryptographicSuitesSupported.IfSome(suites =>
        {
            var suitesJson = new JArray();
            foreach (var suite in suites)
            {
                suitesJson.Add(suite.ToString());
            }
            configJson.Add(CryptographicSuitesSupportedJsonKey, suitesJson);
        });
        
        mdocConfig.CryptographicCurvesSupported.IfSome(curves =>
        {
            var curvesJson = new JArray();
            foreach (var curve in curves)
            {
                curvesJson.Add(curve.ToString());
            }
            configJson.Add(CryptographicCurvesSupportedJsonKey, curvesJson);
        });
        
        mdocConfig.Claims.IfSome(claims =>
            configJson.Add(ClaimsJsonKey, claims.EncodeToJson())
        );

        return configJson;
    }
}
