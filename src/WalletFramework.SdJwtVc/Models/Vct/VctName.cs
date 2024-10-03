using LanguageExt;
using Newtonsoft.Json.Linq;

namespace WalletFramework.SdJwtVc.Models.Vct.Models;

public readonly struct VctName
{
    private string Value { get; }

    private VctName(string value) => Value = value;

    public override string ToString() => Value;
    
    public static implicit operator string(VctName vctName) => vctName.Value;
    
    public static Option<VctName> OptionVctName(JToken vctName)
    {
        var str = vctName.ToString();
        return string.IsNullOrWhiteSpace(str) 
            ? Option<VctName>.None 
            : new VctName(str);
    }
}
