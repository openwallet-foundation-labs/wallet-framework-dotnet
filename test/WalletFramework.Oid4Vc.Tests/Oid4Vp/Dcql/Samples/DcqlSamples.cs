using System.Reflection;
using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;

public static class DcqlSamples
{
    public const string ClaimSetSampleJson = "[\"a\", \"b\", \"c\"]";

    public const string DcqlQueryWithOneCredentialSetJson = @"{
      ""credentials"": [
        {
          ""id"": ""idcard"",
          ""format"": ""dc+sd-jwt"",
          ""meta"": {
            ""vct_values"": [""ID-Card""]
          },
          ""claims"": [
            {""path"": [""first_name""]},
            {""path"": [""last_name""]},
            {""path"": [""address"", ""street_address""]}
          ]
        },
        {
          ""id"": ""idcard2"",
          ""format"": ""dc+sd-jwt"",
          ""meta"": {
            ""vct_values"": [""ID-Card-2""]
          },
          ""claims"": [
            {""path"": [""first_name""]},
            {""path"": [""last_name""]},
            {""path"": [""address"", ""street_address""]}
          ]
        },
        {
          ""id"": ""idcard3"",
          ""format"": ""dc+sd-jwt"",
          ""meta"": {
            ""vct_values"": [""ID-Card-3""]
          },
          ""claims"": [
            {""path"": [""rewards_number""]}
          ]
        }
      ],
      ""credential_sets"": [
        {
          ""options"": [
            [ ""idcard"", ""idcard2"" ]
          ]
        },
        {
          ""required"": false,
          ""options"": [
            [ ""idcard3"" ]
          ]
        }
      ]
    }";

    public const string DcqlQueryWithOneCredentialSetAndMultipleOptionsJson = @"{
  ""credentials"": [
    {
      ""id"": ""idcard"",
      ""format"": ""dc+sd-jwt"",
      ""meta"": {
        ""vct_values"": [""ID-Card""]
      },
      ""claims"": [
        {""path"": [""first_name""]},
        {""path"": [""last_name""]},
        {""path"": [""address"", ""street_address""]}
      ]
    },
    {
      ""id"": ""idcard2"",
      ""format"": ""dc+sd-jwt"",
      ""meta"": {
        ""vct_values"": [""ID-Card-2""]
      },
      ""claims"": [
        {""path"": [""first_name""]},
        {""path"": [""last_name""]},
        {""path"": [""address"", ""street_address""]}
      ]
    },
    {
      ""id"": ""idcard3"",
      ""format"": ""dc+sd-jwt"",
      ""meta"": {
        ""vct_values"": [""ID-Card-3""]
      },
      ""claims"": [
        {""path"": [""rewards_number""]}
      ]
    }
  ],
  ""credential_sets"": [
    {
      ""options"": [
        [ ""idcard"", ""idcard2""],
        [ ""idcard2""]
      ]
    }
  ]
}";

    public const string IdCardAndIdCard2NationalitiesSecondIndexQueryJson = @"{
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

    public const string IdCardNationalitiesSecondIndexQueryJson = @"{
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

    public const string MultipleClaimSetsSampleJson = "[[\"a\", \"b\"], [\"c\", \"d\"]]";

    public const string QueryStrWithClaimSets = @"{
        ""credentials"": [
                {
                    ""id"": ""idcard"",
                    ""format"": ""dc+sd-jwt"",
                    ""meta"": {
                        ""vct_values"": [ ""ID-Card"" ]
                    },
                    ""claims"": [
                        {""id"": {""value"": ""a""}, ""path"": [""last_name""]},
                        {""id"": {""value"": ""b""}, ""path"": [""address"", ""postal_code""]},
                        {""id"": {""value"": ""c""}, ""path"": [""address"", ""street_address""]},
                        {""id"": {""value"": ""d""}, ""path"": [""first_name""]}
                    ],
                    ""claim_sets"": [
                        {""claims"": [""a"", ""b"", ""d""]},
                        {""claims"": [""a"", ""c""]}
                    ]
                }
            ]
        }";

    public const string QueryStrWithNoClaims = @"{
        ""credentials"": [
            {
                ""id"": ""pid"",
                ""format"": ""dc+sd-jwt"",
                ""meta"": {
                    ""vct_values"": [ ""ID-Card"" ]
                }
            }
        ]
    }";

    public static DcqlQuery GetDcqlQueryWithClaimsets =>
        JsonConvert.DeserializeObject<DcqlQuery>(QueryStrWithClaimSets)!;

    public static DcqlQuery GetDcqlQueryWithCredentialSets =>
        JsonConvert.DeserializeObject<DcqlQuery>(DcqlQueryWithOneCredentialSetJson)!;

    public static DcqlQuery GetDcqlQueryWithOneCredentialSet =>
        JsonConvert.DeserializeObject<DcqlQuery>(DcqlQueryWithOneCredentialSetJson)!;

    public static DcqlQuery GetDcqlQueryWithNoClaims =>
        JsonConvert.DeserializeObject<DcqlQuery>(QueryStrWithNoClaims)!;

    public static string GetDcqlQueryAsJsonStr() => GetJsonForTestCase("DcqlQuerySample");

    public static DcqlQuery GetIdCardAndIdCard2NationalitiesSecondIndexQuery() =>
        JsonConvert.DeserializeObject<DcqlQuery>(IdCardAndIdCard2NationalitiesSecondIndexQueryJson)!;

    public static DcqlQuery GetIdCardNationalitiesSecondIndexQuery() =>
        JsonConvert.DeserializeObject<DcqlQuery>(IdCardNationalitiesSecondIndexQueryJson)!;

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
                        { ""path"": [""org.iso.18013.5.1"", ""document_number""] },
                        { ""path"": [""org.iso.18013.5.1"", ""family_name""] }
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

    public static DcqlQuery GetDcqlQueryWithOneCredentialSetAndMultipleOptions =>
        JsonConvert.DeserializeObject<DcqlQuery>(DcqlQueryWithOneCredentialSetAndMultipleOptionsJson)!;

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
