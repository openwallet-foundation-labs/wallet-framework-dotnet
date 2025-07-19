using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;

/// <summary>
///     Represents a single request item in the DC-API batch request.
/// </summary>
public record DcApiRequestItem
{
    /// <summary>
    ///     Gets the data. Contains the actual DcApiRequest.
    /// </summary>
    public AuthorizationRequest Data { get; }

    /// <summary>
    ///     Gets the protocol. Specifies the protocol used for this request.
    /// </summary>
    public string Protocol { get; }
    
    public Option<Origin> Origin { get; init; } = Option<Origin>.None;

    private DcApiRequestItem(
        AuthorizationRequest data,
        string protocol)
    {
        Data = data;
        Protocol = protocol;
    }

    public static Validation<DcApiRequestItem> ValidDcApiRequestItem(JObject requestItemJson)
    {
        var protocolValidation = requestItemJson
            .GetByKey("protocol")
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(jValue =>
            {
                var value = jValue.Value?.ToString();
                return string.IsNullOrWhiteSpace(value)
                    ? new StringIsNullOrWhitespaceError<string>()
                    : Valid(value);
            });

        var dataValidation = requestItemJson
            .GetByKey("data")
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(jObject =>
            {
                var protocol = protocolValidation.Fallback(DcApiConstants.UnsignedProtocol);
                return ProcessAuthRequest(jObject, protocol);
            });

        return Valid(Create)
            .Apply(dataValidation)
            .Apply(protocolValidation);
    }

    private static DcApiRequestItem Create(
        AuthorizationRequest data,
        string protocol) => new(data, protocol);

    private static Validation<AuthorizationRequest> LiftRequest(
        Validation<AuthorizationRequestCancellation, AuthorizationRequest> validation)
    {
        return validation.Match(
            request => request,
            errors =>
            {
                var vpErrors = errors.SelectMany(x => x.Errors);
                return vpErrors.First().ToInvalid<AuthorizationRequest>();
            }
        );
    }

    private static Validation<AuthorizationRequest> ProcessAuthRequest(JObject jObject, string protocol)
    {
        switch (protocol)
        {
            case DcApiConstants.UnsignedProtocol:
                var r = AuthorizationRequest.CreateAuthorizationRequest(jObject);
                return LiftRequest(r);
            case DcApiConstants.SignedProtocol:
                var jToken = jObject.GetByKey("request").UnwrapOrThrow();
                var result =
                    from requestObject in RequestObject.FromStr(jToken.ToString(), Option<string>.None)
                    select requestObject.ToAuthorizationRequest();
                return LiftRequest(result);
            default:
                return new InvalidRequestError($"Invalid protocol: {protocol}");
        }
    }
}
