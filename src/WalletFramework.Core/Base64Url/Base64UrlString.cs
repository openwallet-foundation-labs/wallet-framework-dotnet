using Microsoft.IdentityModel.Tokens;
using WalletFramework.Core.Base64Url.Errors;
using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Base64Url;

public readonly struct Base64UrlString
{
    public byte[] AsBytes => this.ToByteString();
    
    private string Value { get; }

    private Base64UrlString(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Base64UrlString base64UrlString) => base64UrlString.ToString();

    public static Validation<Base64UrlString> ValidBase64UrlString(string base64UrlString)
    {
        byte[] bytes;
        try
        {
            bytes = Base64UrlEncoder.DecodeBytes(base64UrlString);
        }
        catch (Exception e)
        {
            return new Base64UrlError(base64UrlString, e);
        }
        
        return ValidBase64UrlString(bytes);
    }
    
    public static Validation<Base64UrlString> ValidBase64UrlString(IEnumerable<byte> base64UrlBytes)
    {
        try
        {
            var result = Base64UrlEncoder.Encode(base64UrlBytes.ToArray());
            return new Base64UrlString(result);
        }
        catch (Exception e)
        {
            return new Base64UrlError(e);
        }
    }
}    

