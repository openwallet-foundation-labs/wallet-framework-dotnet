namespace WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;

public record Origin(string Value)
{
    public static implicit operator string(Origin origin) => origin.Value;
    
    public static implicit operator Origin(string value) => new(value);
    
    public override string ToString() => Value;
    
    public string AsString() => Value;
}
