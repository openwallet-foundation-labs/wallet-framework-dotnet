using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;

public static class TransactionDataSamples
{
    public const string PaymentTransactionDataForPid =
        "eyJwYXltZW50X2RhdGEiOnsicGF5ZWUiOiJBQkMgQmFuayIsImN1cnJlbmN5X2Ftb3VudCI6eyJjdXJyZW5jeSI6IkVVUiIsInZhbHVlIjoiODAwIn19LCJ0cmFuc2FjdGlvbl9kYXRhX2hhc2hlc19hbGciOlsic2hhLTI1NiJdLCJjcmVkZW50aWFsX2lkcyI6WyJwaWQiXSwidHlwZSI6InBheW1lbnRfZGF0YSJ9";

    private const string AuthRequestWithTransactionDataTemplate = @"{
  ""response_uri"": ""https://test.test.test.io/openid4vp/authorization-response"",
  ""transaction_data"": [
    ""eyJwYXltZW50X2RhdGEiOnsicGF5ZWUiOiJBQkMgQmFuayIsImN1cnJlbmN5X2Ftb3VudCI6eyJjdXJyZW5jeSI6IkVVUiIsInZhbHVlIjoiODAwIn19LCJ0cmFuc2FjdGlvbl9kYXRhX2hhc2hlc19hbGciOlsic2hhLTI1NiJdLCJjcmVkZW50aWFsX2lkcyI6WyJpZGNhcmQiXSwidHlwZSI6InBheW1lbnRfZGF0YSJ9""
  ],
  ""client_id_scheme"": ""x509_san_dns"",
  ""iss"": ""https://test.test.test.io"",
  ""response_type"": ""vp_token"",
  ""nonce"": ""bRlAPdfKK2rSyn8RKoYDkr"",
  ""client_id"": ""test.test.test.io"",
  ""response_mode"": ""direct_post"",
  ""aud"": ""https://self-issued.me/v2"",
  ""dcql_query"": {0},
  ""state"": ""73ec8b46-2289-4a31-856c-06ef56cdf165"",
  ""exp"": 1747316703,
  ""iat"": 1747313103,
  ""client_metadata"": {
    ""client_name"": ""default-test-updated"",
    ""logo_uri"": ""https://www.defaultTestLogo.com/updated-logo.png"",
    ""redirect_uris"": [""https://test.id""],
    ""tos_uri"": ""https://www.example.com/tos"",
    ""policy_uri"": ""https://www.example.com/policy"",
    ""client_uri"": ""https://www.example.com"",
    ""contacts"": [""admin@admin.it""],
    ""vp_formats"": {
      ""vc+sd-jwt"": {
        ""sd-jwt_alg_values"": [""ES256""],
        ""kb-jwt_alg_values"": [""ES256""]
      },
      ""dc+sd-jwt"": {
        ""sd-jwt_alg_values"": [""ES256""],
        ""kb-jwt_alg_values"": [""ES256""]
      }
    }
  }
}";

    public static AuthorizationRequest GetAuthRequestWithSingleCredentialTransactionData()
    {
        var authRequestJson = GetAuthRequestWithSingleCredentialTransactionDataStr();
        return AuthorizationRequest.CreateAuthorizationRequest(authRequestJson).UnwrapOrThrow();
    }

    public static string GetAuthRequestWithSingleCredentialTransactionDataStr()
    {
        return AuthRequestWithTransactionDataTemplate.Replace("{0}", DcqlSamples.IdCardNationalitiesSecondIndexQueryJson);
    }

    public static AuthorizationRequest GetAuthRequestWithTwoCredentialsTransactionData()
    {
        var authRequestJson = GetAuthRequestWithTwoCredentialsTransactionDataStr();
        return AuthorizationRequest.CreateAuthorizationRequest(authRequestJson).UnwrapOrThrow();
    }

    public static string GetAuthRequestWithTwoCredentialsTransactionDataStr()
    {
        return AuthRequestWithTransactionDataTemplate.Replace("{0}", DcqlSamples.IdCardAndIdCard2NationalitiesSecondIndexQueryJson);
    }

    public static AuthorizationRequest GetAuthRequestWithMultipleCandidatesInOneSetTransactionData()
    {
        var authRequestJson = AuthRequestWithTransactionDataTemplate.Replace("{0}", DcqlSamples.DcqlQueryWithOneCredentialSetJson);
        return AuthorizationRequest.CreateAuthorizationRequest(authRequestJson).UnwrapOrThrow();
    }
} 
