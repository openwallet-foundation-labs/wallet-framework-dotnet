using LanguageExt;
using Newtonsoft.Json.Linq;

namespace WalletFramework.SdJwtVc.Models.Vct.Models;

public readonly struct VctDescription
{
    private string Value { get; }

    private VctDescription(string value) => Value = value;

    public override string ToString() => Value;
    
    public static implicit operator string(VctDescription vctDescription) => vctDescription.Value;
    
    public static Option<VctDescription> OptionVctDescription(JToken vctDescription)
    {
        var str = vctDescription.ToString();
        return string.IsNullOrWhiteSpace(str) 
            ? Option<VctDescription>.None 
            : new VctDescription(str);
    }
}
