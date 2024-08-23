using Microsoft.IdentityModel.Tokens;

namespace WalletFramework.Core.Base64Url;

public readonly struct Base64UrlString
{
    private string Value { get; }
    
    public byte[] AsByteArray => Base64UrlEncoder.DecodeBytes(Value);

    public string AsString => Value;

    private Base64UrlString(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Base64UrlString base64UrlString) => base64UrlString.ToString();

    public static Base64UrlString CreateBase64UrlString(IEnumerable<byte> base64UrlBytes)
    {
        var result = Base64UrlEncoder.Encode(base64UrlBytes.ToArray());
        return new Base64UrlString(result);
    }
}    
