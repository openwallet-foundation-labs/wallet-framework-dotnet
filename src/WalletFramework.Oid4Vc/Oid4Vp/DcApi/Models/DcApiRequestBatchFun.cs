using LanguageExt;

namespace WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;

/// <summary>
///     Functions for DcApiRequestBatch.
/// </summary>
public static class DcApiRequestBatchFun
{
    /// <summary>
    ///     Gets the first request in the batch with the specified protocol.
    /// </summary>
    /// <param name="batch">The DC-API request batch.</param>
    /// <returns>The first request item with the specified protocol, or None if not found.</returns>
    public static Option<DcApiRequestItem> GetFirstVpRequest(this DcApiRequestBatch batch)
    {
        var firstRequest = batch.Requests.FirstOrDefault(request => request.Protocol.Contains("openid4vp"));
        return firstRequest ?? Option<DcApiRequestItem>.None;
    }
} 
