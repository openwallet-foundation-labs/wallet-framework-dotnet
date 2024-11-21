using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Security.Cose.Errors;

namespace WalletFramework.MdocLib.Security.Cose;

public enum CoseEllipticCurves
{
    P256 = 1,
    P384 = 2,
    P521 = 3,
    X25519 = 4,
    X448 = 5,
    Ed25519 = 6,
    Ed448 = 7,
    Secp256K1 = 8,
    BrainPoolP256R1 = 256,
    BrainPoolP320R1 = 257,
    BrainPoolP384R1 = 258,
    BrainPoolP512R1 = 259
}

public static class CoseEllipticCurvesFun
{
    public static CBORObject ToCbor(this CoseEllipticCurves curves) => CBORObject.FromObject((int)curves);

    public static Validation<CoseEllipticCurves> FromCbor(CBORObject cbor)
    {
        var curve = cbor.AsInt32();
        if (curve is 1)
        {
            return CoseEllipticCurves.P256;
        }
        else
        {
            return new NotSupportedCurveError(curve.ToString());
        }
    }
}
