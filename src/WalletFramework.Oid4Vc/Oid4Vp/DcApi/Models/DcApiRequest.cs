using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Errors;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;

/// <summary>
///     Represents a request in the DC-API flow.
/// </summary>
public record DcApiRequest
{
    /// <summary>
    ///     Gets the DCQL query. Contains the claims that the Verifier wants to receive.
    /// </summary>
    public DcqlQuery DcqlQuery { get; }

    /// <summary>
    ///     Gets the nonce. Random string for session binding.
    /// </summary>
    public string Nonce { get; }

    /// <summary>
    ///     Gets the response mode. Determines how the response should be sent.
    /// </summary>
    public string ResponseMode { get; }

    /// <summary>
    ///     Gets the response type. Specifies the type of response expected.
    /// </summary>
    public string ResponseType { get; }

    private DcApiRequest(
        DcqlQuery dcqlQuery,
        string nonce,
        string responseMode,
        string responseType)
    {
        DcqlQuery = dcqlQuery;
        Nonce = nonce;
        ResponseMode = responseMode;
        ResponseType = responseType;
    }

    private static DcApiRequest Create(
        DcqlQuery dcqlQuery,
        string nonce,
        string responseMode,
        string responseType) => new(dcqlQuery, nonce, responseMode, responseType);

    public static Validation<DcApiRequest> ValidDcApiRequest(string requestJson)
    {
        if (string.IsNullOrWhiteSpace(requestJson))
        {
            return new StringIsNullOrWhitespaceError<DcApiRequest>();
        }

        JObject jObject;
        try
        {
            jObject = JObject.Parse(requestJson);
        }
        catch (Exception e)
        {
            return new InvalidJsonError(requestJson, e).ToInvalid<DcApiRequest>();
        }
        
        return ValidDcApiRequest(jObject);
    }

    public static Validation<DcApiRequest> ValidDcApiRequest(JObject requestJson)
    {
        var dcqlQueryValidation = requestJson
            .GetByKey("dcql_query")
            .OnSuccess(token =>
            {
                var result = token.ToObject<DcqlQuery>();
                if (result == null)
                {
                    return new InvalidRequestError("Could not parse DCQL query").ToInvalid<DcqlQuery>();
                }
                else
                {
                    return result;
                }
            });

        var nonceValidation = requestJson
            .GetByKey("nonce")
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(jValue =>
            {
                var value = jValue.Value?.ToString();
                return string.IsNullOrWhiteSpace(value)
                    ? new StringIsNullOrWhitespaceError<string>()
                    : Valid(value);
            });

        var responseModeValidation = requestJson
            .GetByKey("response_mode")
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(jValue =>
            {
                var value = jValue.Value?.ToString();
                return string.IsNullOrWhiteSpace(value)
                    ? new StringIsNullOrWhitespaceError<string>()
                    : Valid(value);
            });

        var responseTypeValidation = requestJson
            .GetByKey("response_type")
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(jValue =>
            {
                var value = jValue.Value?.ToString();
                return string.IsNullOrWhiteSpace(value)
                    ? new StringIsNullOrWhitespaceError<string>()
                    : Valid(value);
            });

        return Valid(Create)
            .Apply(dcqlQueryValidation)
            .Apply(nonceValidation)
            .Apply(responseModeValidation)
            .Apply(responseTypeValidation);
    }
} 
