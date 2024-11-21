using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Cbor.Abstractions;
using WalletFramework.MdocLib.Device.Errors;

namespace WalletFramework.MdocLib.Security.Cose;

public record CoseKey(PublicKey Value) : ICborSerializable
{
    public static Validation<CoseKey> FromCborBytes(CBORObject bytes) =>
        from byteString in CborByteString.ValidCborByteString(bytes)
        from key in FromCbor(byteString.Decode())
        select key;

    public static Validation<CoseKey> FromCbor(CBORObject deviceKey)
    {
        var ktyLabel = CBORObject.FromObject(1);
        var validKty = deviceKey.GetByLabel(ktyLabel).OnSuccess(keyType =>
        {
            int keyTypeValue;
            try
            {
                keyTypeValue = keyType.AsNumber().ToInt32Checked();
            }
            catch (Exception e)
            {
                return new CborIsNotANumberError(keyType.ToString(), e);
            }
            
            if (keyTypeValue != 2)
            {
                return new UnsupportedKeyTypeError(keyTypeValue);
            }

            return Unit.Default;
        });

        var crvLabel = CBORObject.FromObject(-1);
        var validCrv = deviceKey.GetByLabel(crvLabel).OnSuccess(curve =>
        {
            int curveValue;
            try
            {
                curveValue = curve.AsNumber().ToInt32Checked();
            }
            catch (Exception e)
            {
                return new CborIsNotANumberError(curve.ToString(), e);
            }
            
            if (curveValue != 1)
            {
                return new UnsupportedCurveError(curveValue);
            }

            return Unit.Default;
        });

        var xLabel = CBORObject.FromObject(-2);
        var validX =
            from xValue in deviceKey.GetByLabel(xLabel)
            from byteString in xValue.TryGetByteString()
            select Base64UrlString.CreateBase64UrlString(byteString);

        var yLabel = CBORObject.FromObject(-3);
        var validY =
            from yValue in deviceKey.GetByLabel(yLabel)
            from byteString in yValue.TryGetByteString()
            select Base64UrlString.CreateBase64UrlString(byteString);

        return
            from kty in validKty
            from crv in validCrv
            from x in validX
            from y in validY
            let pubKey = new PublicKey(x, y)
            select new CoseKey(pubKey);
    }

    public CBORObject ToCbor()
    {
        var result = CBORObject.NewMap();

        var ktyLabel = CBORObject.FromObject(1);
        var kty = CBORObject.FromObject(2);
        result.Add(ktyLabel, kty);

        var crvLabel = CBORObject.FromObject(-1);
        var crv = CBORObject.FromObject(1);
        result.Add(crvLabel, crv);

        var xLabel = CBORObject.FromObject(-2);
        var x = Value.X.AsByteArray;
        result.Add(xLabel, x);
        
        var yLabel = CBORObject.FromObject(-3);
        var y = Value.Y.AsByteArray;
        result.Add(yLabel, y);

        return result;
    }
}

public static class CoseKeyFun
{
    public static CoseKey ToCoseKey(this PublicKey publicKey) => new(publicKey);
}
