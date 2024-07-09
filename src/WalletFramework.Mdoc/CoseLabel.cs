using Newtonsoft.Json.Linq;
using PeterO.Cbor;
using WalletFramework.Functional;

namespace WalletFramework.Mdoc;

public readonly struct CoseLabel
{
    private int Id { get; }

    public string Value => Id.ToString();

    private CoseLabel(int id)
    {
        Id = id;
    }
    
    public static implicit operator string(CoseLabel label) => label.Value;
    
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
