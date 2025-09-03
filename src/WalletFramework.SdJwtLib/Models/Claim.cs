namespace WalletFramework.SdJwtLib.Models;

public class Claim
{
    public string Name;
    public object Value;
    public bool NonDisclosable;

    public Claim(string name, object value, bool nonDisclosable = false)
    {
        Name = name;
        Value = value;
        NonDisclosable = nonDisclosable;
    }
}
