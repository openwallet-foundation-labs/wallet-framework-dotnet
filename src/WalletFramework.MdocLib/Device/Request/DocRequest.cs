using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Cbor.Abstractions;

namespace WalletFramework.MdocLib.Device.Request;

public record DocRequest(ItemsRequest ItemsRequest) : ICborSerializable
{
    public CBORObject ToCbor()
    {
        var cbor = CBORObject.NewMap();
        
        cbor.Add("itemsRequest", ItemsRequest.ToCbor());

        return cbor;
    }

    public static Validation<DocRequest> FromCbor(CBORObject cbor)
    {
        var validItemsRequest =
            from itemsRequestCbor in cbor.GetByLabel("itemsRequest")
            from itemsRequest in ItemsRequest.FromCbor(itemsRequestCbor)
            select itemsRequest;

        return
            from itemsRequest in validItemsRequest
            select new DocRequest(itemsRequest);
    }
}

public static class DocRequestFun
{
    // public static Validation<IEnumerable<DocRequest>> ValidDocRequests(CBORObject docRequestsCbor)
    // {
    //     
    // }
    
    public static DocRequest ToDocRequest(this ItemsRequest itemsRequest) => new(itemsRequest);
}
