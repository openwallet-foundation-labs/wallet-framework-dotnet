using PeterO.Cbor;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Device.Errors;

namespace WalletFramework.MdocLib.Reader;

public record EngagementUri(Base64UrlString Value, CBORObject AsCbor)
{
    public const string Scheme = "mdoc://";
    
    public static Validation<EngagementUri> FromString(string input)
    {
        if (!input.StartsWith(Scheme))
            return new InvalidEngagementUriSchemeError(Scheme, input);

        var path = input[Scheme.Length..];

        return 
            from base64Url in Base64UrlString.FromString(path)
            from cbor in CborFun.Decode(base64Url.AsByteArray)
            select new EngagementUri(base64Url, cbor);
    }
}
