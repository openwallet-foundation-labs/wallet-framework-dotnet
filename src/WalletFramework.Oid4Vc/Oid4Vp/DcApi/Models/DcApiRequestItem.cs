using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
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
    // public DcApiRequest Data { get; }
    public AuthorizationRequest Data { get; }

    /// <summary>
    ///     Gets the protocol. Specifies the protocol used for this request.
    /// </summary>
    public string Protocol { get; }

    private DcApiRequestItem(
        // DcApiRequest data,
        AuthorizationRequest data,
        string protocol)
    {
        Data = data;
        Protocol = protocol;
    }

    private static Validation<AuthorizationRequest> Bla(
        Validation<AuthorizationRequestCancellation, AuthorizationRequest> validation)
    {
        return validation.Match(
            request =>
            {
                return request;
            },
            errors =>
            {
                throw new InvalidOperationException();
            }
        );
    }

    public static Validation<DcApiRequestItem> ValidDcApiRequestItem(JObject requestItemJson)
    {
        var dataValidation = requestItemJson
            .GetByKey("data")
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(x =>
            {
                return Bla(AuthorizationRequest.CreateAuthorizationRequest(x));
            });

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
        
        return Valid(Create)
            .Apply(dataValidation)
            .Apply(protocolValidation);
    }

    private static DcApiRequestItem Create(
        AuthorizationRequest data,
        string protocol) => new(data, protocol);
}
