using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Uri;
using WalletFramework.SdJwtVc.Models.Vct.Errors;

namespace WalletFramework.SdJwtVc.Models.Vct.Models;

public readonly struct VctExtends
{
    private Uri Value { get; }
    
    private VctExtends(Uri value) => Value = value;

    public override string ToString() => Value.ToStringWithoutTrail();
    
    public static implicit operator Uri(VctExtends vctExtends) => vctExtends.Value;

    public static Validation<VctExtends> ValidVctExtends(JToken vctExtends) => vctExtends.ToJValue().OnSuccess(value =>
    {
        try
        {
            var str = value.ToString(CultureInfo.InvariantCulture);
            var uri = new Uri(str);
            return new VctExtends(uri);
        }
        catch (Exception e)
        {
            return new VctExtendsError(e).ToInvalid<VctExtends>();
        }
    });
}
