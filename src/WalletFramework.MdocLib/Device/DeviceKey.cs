using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Device.Errors;

namespace WalletFramework.MdocLib.Device;

public record DeviceKey(PublicKey Value)
{
    public static Validation<DeviceKey> ValidDeviceKey(CBORObject deviceKey)
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
            select new DeviceKey(pubKey);
    }
}
