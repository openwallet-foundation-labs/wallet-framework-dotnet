using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Errors;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;

/// <summary>
///     Represents a batch of DC-API requests.
/// </summary>
public record DcApiRequestBatch
{
    /// <summary>
    ///     Gets the requests. Contains an array of DC-API request items.
    /// </summary>
    public DcApiRequestItem[] Requests { get; }

    private DcApiRequestBatch(DcApiRequestItem[] requests)
    {
        Requests = requests;
    }

    private static DcApiRequestBatch Create(DcApiRequestItem[] requests) => new(requests);

    public static Validation<DcApiRequestBatch> From(string requestBatchJson)
    {
        if (string.IsNullOrWhiteSpace(requestBatchJson))
        {
            return new StringIsNullOrWhitespaceError<DcApiRequestBatch>();
        }

        JObject jObject;
        try
        {
            jObject = JObject.Parse(requestBatchJson);
        }
        catch (Exception e)
        {
            return new InvalidJsonError(requestBatchJson, e).ToInvalid<DcApiRequestBatch>();
        }
        
        return From(jObject);
    }

    public static Validation<DcApiRequestBatch> From(JObject requestBatchJson)
    {
        var requestsValidation =
            from jToken in requestBatchJson.GetByKey("requests")
            from jArray in jToken.ToJArray()
            from items in jArray.TraverseAll(token =>
            {
                return
                    from jObject in token.ToJObject()
                    from item in DcApiRequestItem.ValidDcApiRequestItem(jObject)
                    select item;
            })
            select items.ToArray();

        return Valid(Create).Apply(requestsValidation);
    }
} 
