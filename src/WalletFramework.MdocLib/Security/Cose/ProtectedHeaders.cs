using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using static WalletFramework.MdocLib.Security.Cose.ProtectedHeaders.Alg;
using static WalletFramework.MdocLib.Cbor.CborByteString;

namespace WalletFramework.MdocLib.Security.Cose;

public readonly struct ProtectedHeaders
{
    public Dictionary<CoseLabel, Alg> Value { get; }

    public CborByteString AsCborByteString { get; }

    public Alg this[CoseLabel key] => Value[key];

    private ProtectedHeaders(Dictionary<CoseLabel, Alg> value, CborByteString asCborByteString)
    {
        Value = value;
        AsCborByteString = asCborByteString;
    }

    internal static Validation<ProtectedHeaders> ValidProtectedHeaders(CBORObject issuerAuth) => issuerAuth
        .GetByIndex(0)
        .OnSuccess(ValidCborByteString)
        .OnSuccess(byteString =>
        {
            var decoded = byteString.Decode();
            return decoded.Values.Count > 1
                ? new ProtectedHeadersMustOnlyContainOneElementError()
                : decoded.ToDictionary(ValidAlgLabel, ValidAlg).OnSuccess(algs =>
                    new ProtectedHeaders(algs, byteString)
                );
        });

    private static Validation<CoseLabel> ValidAlgLabel(CBORObject cbor)
    {
        var validateAlgLabel = new Func<CoseLabel, Validation<CoseLabel>>(label =>
        {
            if (label.Value == "1")
            {
                return label;
            }
            else
            {
                return new InvalidAlgLabelError();
            }
        });

        var result =
            from label in CoseLabel.ValidCoseLabel(cbor)
            select validateAlgLabel(label);

        return result.Flatten();
    }

    public static ProtectedHeaders BuildProtectedHeaders(AlgValue algValue = AlgValue.Es256)
    {
        var algLabel = new CoseLabel(1);
        var alg = new Alg(algValue);
        var dict = new Dictionary<CoseLabel, Alg> { { algLabel, alg } };
        
        var cbor = CBORObject.NewMap();
        cbor.Add(algLabel.AsCbor, alg.AsCbor);

        return new ProtectedHeaders(dict, cbor.ToCborByteString());
    }

    public record ProtectedHeadersMustOnlyContainOneElementError()
        : Error("ProtectedHeaders must only contain one element which is the alg element");

    public record InvalidAlgLabelError() : Error("Invalid Label for Alg. Must be 1");

    public readonly struct Alg
    {
        public AlgValue Value { get; }

        public Alg(AlgValue str) => Value = str;
        
        public CBORObject AsCbor => Value switch
        {
            AlgValue.Es256 => CBORObject.FromObject(-7),
            AlgValue.Es384 => CBORObject.FromObject(-35),
            AlgValue.Es512 => CBORObject.FromObject(-36),
            AlgValue.Eddsa => CBORObject.FromObject(-8),
            _ => throw new ArgumentOutOfRangeException()
        };

        internal static Validation<Alg> ValidAlg(CBORObject cbor)
        {
            int alg;
            try
            {
                alg = cbor.AsNumber().ToInt32Checked();
            }
            catch (Exception e)
            {
                return new CborIsNotATextStringError("alg", e);
            }

            return alg switch
            {
                -7 => new Alg(AlgValue.Es256),
                -35 => new Alg(AlgValue.Es384),
                -36 => new Alg(AlgValue.Es512),
                -8 => new Alg(AlgValue.Eddsa),
                _ => new InvalidAlgError(alg)
            };
        }

        public enum AlgValue
        {
            Es256,
            Es384,
            Es512,
            Eddsa
        }

        public override string ToString() => Value switch
        {
            AlgValue.Es256 => "ES256",
            AlgValue.Es384 => "ES384",
            AlgValue.Es512 => "ES512",
            AlgValue.Eddsa => "EdDSA",
            _ => throw new ArgumentOutOfRangeException()
        };

        public record InvalidAlgError(int Value)
            : Error($"Invalid Alg. Must be -7(ES256), (-35)ES384, -36(ES512), or -8(EdDSA). Got {Value}");
    }
}
