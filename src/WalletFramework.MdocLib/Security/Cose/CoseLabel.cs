using Newtonsoft.Json.Linq;
using PeterO.Cbor;
using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Security.Cose;

public readonly struct CoseLabel
{
    public CBORObject AsCbor => CBORObject.FromObject(Id);
    
    private int Id { get; }

    public string Value => Id.ToString();

    public CoseLabel(int id)
    {
        Id = id;
    }

    public static implicit operator int(CoseLabel label) => int.Parse(label.Value);
    
    public static implicit operator string(CoseLabel label) => label.Value;

    public static implicit operator CBORObject(CoseLabel label) => label.AsCbor;
    
    public override string ToString() => Value;

    internal static Validation<CoseLabel> ValidCoseLabel(CBORObject cbor)
    {
        int id;
        try
        {
            id = cbor.AsInt32();
        }
        catch (Exception e)
        {
            return new CoseLabelIsNotANumberError(e);
        }

        return new CoseLabel(id);
    }
    
    public static Validation<CoseLabel> ValidCoseLabel(JToken token)
    {
        int id;
        try
        {
            var str = token.ToString();
            id = int.Parse(str);
        }
        catch (Exception e)
        {
            return new CoseLabelIsNotANumberError(e);
        }

        return new CoseLabel(id);
    }
    
    public record CoseLabelIsNotANumberError(Exception E) : Error("CBOR is not an integer", E);
}
