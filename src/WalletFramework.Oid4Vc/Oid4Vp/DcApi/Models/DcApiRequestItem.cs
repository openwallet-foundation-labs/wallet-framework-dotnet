using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
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
    public DcApiRequest Data { get; }

    /// <summary>
    ///     Gets the protocol. Specifies the protocol used for this request.
    /// </summary>
    public string Protocol { get; }

    private DcApiRequestItem(
        DcApiRequest data,
        string protocol)
    {
        Data = data;
        Protocol = protocol;
    }

    public static Validation<DcApiRequestItem> ValidDcApiRequestItem(JObject requestItemJson)
    {
        var dataValidation = requestItemJson
            .GetByKey("data")
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(DcApiRequest.ValidDcApiRequest);

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
        DcApiRequest data,
        string protocol) => new(data, protocol);
}
