using OneOf;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Device.Request;
using WalletFramework.MdocLib.Device.Response;
using WalletFramework.MdocLib.Security.Errors;

namespace WalletFramework.MdocLib.Security;

public record SessionData(OneOf<EncryptedDeviceRequest, EncryptedDeviceResponse> Data);

public static class SessionDataFun
{
    public static CBORObject ToCbor(this SessionData data)
    {
        var result = CBORObject.NewMap();

        var dataLabel = CBORObject.FromObject("data");
        data.Data.Match(
            response => result.Add(dataLabel, response.Encrypted),
            request => result.Add(dataLabel, request.Encrypted)
        );

        return result;
    }

    public static Validation<SessionData> FromCbor(CBORObject sessionDataCbor, byte[] derivatedKey,
        IAesGcmEncryption aes)
    {
        var data = sessionDataCbor.GetByLabel("data").OnSuccess(dataCbor =>
        {
            var bytes = dataCbor.GetByteString();

            var encryptedRequest = EncryptedDeviceRequest.FromBytes(bytes, derivatedKey, aes);
            if (encryptedRequest.IsSuccess)
            {
                return encryptedRequest.Select(request =>
                    (OneOf<EncryptedDeviceRequest, EncryptedDeviceResponse>)request);
            }
            
            var encryptedResponse = EncryptedDeviceResponse.FromBytes(bytes, derivatedKey, aes);
            if (encryptedResponse.IsSuccess)
            {
                return encryptedResponse.Select(response =>
                    (OneOf<EncryptedDeviceRequest, EncryptedDeviceResponse>)response);
            }

            return new SessionDataError();
        });
        
        return
            from encrypted in data
            select new SessionData(encrypted);
    }
}
