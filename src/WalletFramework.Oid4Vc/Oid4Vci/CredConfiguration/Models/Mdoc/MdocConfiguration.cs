using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.ClaimPaths.Errors;
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

    public Format Format => CredentialConfiguration.Format;

    private MdocConfiguration(
        CredentialConfiguration credentialConfiguration,
        DocType docType,
        Option<Policy> policy,
        Option<List<CryptographicSuite>> cryptographicSuitesSupported,
        Option<List<CryptographicCurve>> cryptographicCurvesSupported)
    {
        CredentialConfiguration = credentialConfiguration;
        DocType = docType;
        Policy = policy;
        CryptographicSuitesSupported = cryptographicSuitesSupported;
        CryptographicCurvesSupported = cryptographicCurvesSupported;
    }

    private static MdocConfiguration Create(
        CredentialConfiguration credentialConfiguration,
        DocType docType,
        Option<Policy> policy,
        Option<List<CryptographicSuite>> cryptographicSuitesSupported,
        Option<List<CryptographicCurve>> cryptographicCurvesSupported) =>
        new(credentialConfiguration, docType, policy, cryptographicSuitesSupported, cryptographicCurvesSupported);
    
    public static Validation<MdocConfiguration> ValidMdocConfiguration(JObject config)
    {
        var mDocClaimPathValidation = new Func<JToken, Validation<ClaimPath>>(token =>
        {
            var mDocClaimPath =
                from jArray in token.ToJArray()
                from claimPath in ClaimPath.FromJArray(jArray)
                let pathComponents = claimPath.GetPathComponents()
                from _ in pathComponents.Count > 2
                          && pathComponents[0].IsKey
                          && pathComponents[1].IsKey
                    ? ValidationFun.Valid(Unit.Default)
                    : new UnknownComponentError()
                select claimPath;

            return mDocClaimPath;
        });
        
        var credentialConfiguration = CredentialConfiguration.ValidCredentialConfiguration(config, mDocClaimPathValidation);
        
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

        return ValidationFun.Valid(Create)
            .Apply(credentialConfiguration)
            .Apply(docType)
            .Apply(policy)
            .Apply(cryptographicSuitesSupported)
            .Apply(cryptographicCurvesSupported);
    }
}

public static class MdocConfigurationFun
{
    public const string DocTypeJsonKey = "doctype";
    public const string PolicyJsonKey = "policy";
    public const string CryptographicSuitesSupportedJsonKey = "cryptographic_suites_supported";
    public const string CryptographicCurvesSupportedJsonKey = "cryptographic_curves_supported";

    public static JObject EncodeToJson(this MdocConfiguration mDocConfig)
    {
        var configJson = mDocConfig.CredentialConfiguration.EncodeToJson();

        configJson.Add(DocTypeJsonKey, mDocConfig.DocType.ToString());

        mDocConfig.Policy.IfSome(policy => 
            configJson.Add(PolicyJsonKey, policy.EncodeToJson())
        );
        
        mDocConfig.CryptographicSuitesSupported.IfSome(suites =>
        {
            var suitesJson = new JArray();
            foreach (var suite in suites)
            {
                suitesJson.Add(suite.ToString());
            }
            configJson.Add(CryptographicSuitesSupportedJsonKey, suitesJson);
        });
        
        mDocConfig.CryptographicCurvesSupported.IfSome(curves =>
        {
            var curvesJson = new JArray();
            foreach (var curve in curves)
            {
                curvesJson.Add(curve.ToString());
            }
            configJson.Add(CryptographicCurvesSupportedJsonKey, curvesJson);
        });

        return configJson;
    }
}
