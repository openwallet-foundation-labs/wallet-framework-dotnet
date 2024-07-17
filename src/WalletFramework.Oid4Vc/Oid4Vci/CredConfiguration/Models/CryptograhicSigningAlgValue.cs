using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

public readonly struct CryptograhicSigningAlgValue
{
    private string Value { get; }

    private CryptograhicSigningAlgValue(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(CryptograhicSigningAlgValue alg) => alg.ToString();
    
    public static Validation<CryptograhicSigningAlgValue> ValidCryptograhicSigningAlgValue(JToken alg) => alg.ToJValue().OnSuccess(value =>
    {
        var str = value.ToString(CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(str))
        {
            return new StringIsNullOrWhitespaceError<CryptograhicSigningAlgValue>();
        }

        return Valid(new CryptograhicSigningAlgValue(str));
    });
}
