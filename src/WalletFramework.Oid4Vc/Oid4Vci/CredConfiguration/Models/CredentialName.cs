using System.Globalization;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

public readonly struct CredentialName
{
    private string Value { get; }

    private CredentialName(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(CredentialName credentialName) => credentialName.ToString();

    public static Option<CredentialName> OptionalCredentialName(JToken name) => name.ToJValue().ToOption().OnSome(value =>
    {
        var str = value.ToString(CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(str))
        {
            return Option<CredentialName>.None;
        }

        return new CredentialName(str);
    });
}
