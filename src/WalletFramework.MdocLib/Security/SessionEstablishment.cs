using PeterO.Cbor;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Device.Request;
using WalletFramework.MdocLib.Security.Cose;
using WalletFramework.MdocLib.Security.Errors;

namespace WalletFramework.MdocLib.Security;

public record SessionEstablishment(PublicKey EReaderKey, EncryptedDeviceRequest Data);

public static class SessionEstablishmentFun
{
    public static CBORObject ToCbor(this SessionEstablishment data)
    {
        var result = CBORObject.NewMap();

        var keyLabel = CBORObject.FromObject("eReaderKey");
        result.Add(keyLabel, data.EReaderKey.ToCoseKey().ToCbor().ToTaggedCborByteString());

        var dataLabel = CBORObject.FromObject("data");
        result.Add(dataLabel, data.Data.Encrypted);

        return result;
    }

    public static Validation<SessionEstablishment> FromCbor(CBORObject sessionEstablishmentCbor, byte[] derivatedKey,
        IAesGcmEncryption aes)
    {
        var data = sessionEstablishmentCbor.GetByLabel("data").OnSuccess(dataCbor =>
        {
            var bytes = dataCbor.GetByteString();

            var encryptedRequest = EncryptedDeviceRequest.FromBytes(bytes, derivatedKey, aes);
            if (encryptedRequest.IsSuccess)
                return encryptedRequest.Select(request =>
                    request);

            return new SessionEstablishmentError();
        });
        var eReaderKey = sessionEstablishmentCbor.GetByLabel("eReaderKey").OnSuccess(dataCbor =>
        {
            var eReaderKey = CoseKey.FromCborBytes(dataCbor);

            return eReaderKey.IsSuccess ? eReaderKey : new SessionEstablishmentError();
        });

        return
            from encrypted in data
            from key in eReaderKey
            select new SessionEstablishment(key.Value, encrypted);
    }
}
