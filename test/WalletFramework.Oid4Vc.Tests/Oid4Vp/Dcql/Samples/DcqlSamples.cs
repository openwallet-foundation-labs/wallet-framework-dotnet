using System.Reflection;
using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;

public static class DcqlSamples
{
    public static string GetDcqlQueryAsJsonStr() => GetJsonForTestCase("DcqlQuerySample");

    public static DcqlQuery GetIdCardNationalitiesSecondIndexQuery()
    {
        var json = @"{
            ""credentials"": [
                {
                    ""id"": ""idcard"",
                    ""format"": ""dc+sd-jwt"",
                    ""meta"": {
                        ""vct_values"": [""ID-Card""]
                    },
                    ""claims"": [
                        { ""path"": [""nationalities"", 1] }
                    ]
                }
            ]
        }";

        return JsonConvert.DeserializeObject<DcqlQuery>(json)!;
    }

    public static DcqlQuery GetIdCardAndIdCard2NationalitiesSecondIndexQuery()
    {
        var json = @"{
            ""credentials"": [
                {
                    ""id"": ""idcard1"",
                    ""format"": ""dc+sd-jwt"",
                    ""meta"": {
                        ""vct_values"": [""ID-Card""]
                    },
                    ""claims"": [
                        { ""path"": [""nationalities"", 1] }
                    ]
                },
                {
                    ""id"": ""idcard2"",
                    ""format"": ""dc+sd-jwt"",
                    ""meta"": {
                        ""vct_values"": [""ID-Card-2""]
                    },
                    ""claims"": [
                        { ""path"": [""nationalities"", 1] }
                    ]
                }
            ]
        }";

        return JsonConvert.DeserializeObject<DcqlQuery>(json)!;
    }

    public static DcqlQuery GetNoMatchErrorClaimPathQuery()
    {
        var json = @"{
            ""credentials"": [
                {
                    ""id"": ""idcard"",
                    ""format"": ""dc+sd-jwt"",
                    ""meta"": {
                        ""vct_values"": [""ID-Card""]
                    },
                    ""claims"": [
                        { ""path"": [""ERROR""] }
                    ]
                }
            ]
        }";

        return JsonConvert.DeserializeObject<DcqlQuery>(json)!;
    }

    public static DcqlQuery GetMdocGivenNameQuery()
    {
        var json = @"{
            ""credentials"": [
                {
                    ""id"": ""mdoc"",
                    ""format"": ""mso_mdoc"",
                    ""meta"": {
                        ""doctype_value"": ""org.iso.18013.5.1.mDL""
                    },
                    ""claims"": [
                        { ""path"": [""org.iso.18013.5.1"", ""family_name""] }
                    ]
                }
            ]
        }";
        return JsonConvert.DeserializeObject<DcqlQuery>(json)!;
    }

    public static DcqlQuery GetMdocGivenNameAndFamilyNameQuery()
    {
        var json = @"{
            ""credentials"": [
                {
                    ""id"": ""mdoc"",
                    ""format"": ""mso_mdoc"",
                    ""meta"": {
                        ""doctype_value"": ""org.iso.18013.5.1.mDL""
                    },
                    ""claims"": [
                        { ""path"": [""org.iso.18013.5.1"", ""given_name""] },
                        { ""path"": [""org.iso.18013.5.1"", ""family_name""] }
                    ]
                }
            ]
        }";
        return JsonConvert.DeserializeObject<DcqlQuery>(json)!;
    }

    public static DcqlQuery GetMdocAndSdJwtFamilyNameQuery()
    {
        var json = @"{
            ""credentials"": [
                {
                    ""id"": ""mdoc"",
                    ""format"": ""mso_mdoc"",
                    ""meta"": {
                        ""doctype_value"": ""org.iso.18013.5.1.mDL""
                    },
                    ""claims"": [
                        { ""path"": [""org.iso.18013.5.1"", ""family_name""] }
                    ]
                },
                {
                    ""id"": ""idcard"",
                    ""format"": ""dc+sd-jwt"",
                    ""meta"": {
                        ""vct_values"": [""ID-Card""]
                    },
                    ""claims"": [
                        { ""path"": [""last_name""] }
                    ]
                }
            ]
        }";
        return JsonConvert.DeserializeObject<DcqlQuery>(json)!;
    }

    private static string GetJsonForTestCase(string name = "")
    {
        var assembly = Assembly.GetExecutingAssembly();
        var currentNamespace = typeof(DcqlSamples).Namespace;
        var resourceName = $"{currentNamespace}.{name}.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find resource with name {resourceName}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
