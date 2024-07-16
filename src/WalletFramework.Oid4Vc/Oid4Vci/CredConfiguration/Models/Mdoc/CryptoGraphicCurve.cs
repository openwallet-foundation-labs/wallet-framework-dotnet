using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json.Converters;
using WalletFramework.MdocLib;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

[JsonConverter(typeof(ValueTypeJsonConverter<CryptographicCurve>))]
public readonly struct CryptographicCurve
{
    public CoseLabel Value { get; }

    private CryptographicCurve(CoseLabel value) => Value = value;

    public static Validation<CryptographicCurve> ValidCryptographicCurve(JToken curve) =>
        CoseLabel.ValidCoseLabel(curve).OnSuccess(label => new CryptographicCurve(label));

    public override string ToString() => Value;
}
