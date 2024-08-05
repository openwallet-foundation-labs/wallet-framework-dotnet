using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using PeterO.Cbor;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Versioning;
using WalletFramework.MdocLib.Cbor;
using WalletFramework.MdocLib.Digests;
using static WalletFramework.MdocLib.Digests.DigestAlgorithm;
using static WalletFramework.MdocLib.DocType;
using static WalletFramework.MdocLib.Security.ValidityInfo;
using static WalletFramework.MdocLib.Cbor.CborByteString;
using static WalletFramework.MdocLib.Digests.ValueDigests;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.MdocLib.Security;

public record MobileSecurityObject
{
    public CborByteString ByteString { get; }

    public Version Version { get; }

    public DigestAlgorithm DigestAlgorithm { get; }

    public ValueDigests ValueDigests { get; }

    public DocType DocType { get; }

    public ValidityInfo ValidityInfo { get; }

    // TODO: Implement DeviceKeyInfo
    // public DeviceKeyInfo DeviceKeyInfo { get; init; }

    private MobileSecurityObject(
        CborByteString byteString,
        Version version,
        DigestAlgorithm digestAlgorithm,
        ValueDigests valueDigests,
        DocType docType,
        ValidityInfo validityInfo)
    {
        ByteString = byteString;
        Version = version;
        DigestAlgorithm = digestAlgorithm;
        ValueDigests = valueDigests;
        DocType = docType;
        ValidityInfo = validityInfo;
    }

    private static MobileSecurityObject Create(
        CborByteString byteString,
        Version version,
        DigestAlgorithm digestAlgorithm,
        ValueDigests valueDigests,
        DocType docType,
        ValidityInfo validityInfo) =>
        new(byteString, version, digestAlgorithm, valueDigests, docType, validityInfo);

    internal static Validation<MobileSecurityObject> ValidMobileSecurityObject(CBORObject issuerAuth) =>
        from payloadEncoded in issuerAuth.GetByIndex(2)
        from payloadEncodedByteString in ValidCborByteString(payloadEncoded)
        let payloadDecoded = payloadEncodedByteString.Decode()
        from taggedByteString in ValidCborByteString(payloadDecoded)
        let mso = taggedByteString.Decode()
        from result in Valid(Create)
            .Apply(payloadEncodedByteString)
            .Apply(ValidMsoVersion(mso))
            .Apply(ValidDigestAlgorithm(mso))
            .Apply(mso.GetByLabel("valueDigests").OnSuccess(ValidValueDigests))
            .Apply(ValidDoctype(mso))
            .Apply(ValidValidityInfo(mso))
        select result;

    private static Validation<Version> ValidMsoVersion(CBORObject issuerAuth) =>
        issuerAuth.GetByLabel("version").OnSuccess(version =>
        {
            string str;
            try
            {
                str = version.AsString();
            }
            catch (Exception e)
            {
                return new CborIsNotATextStringError("version", e).ToInvalid<Version>();
            }

            try
            {
                return new Version(version.AsString());
            }
            catch (Exception e)
            {
                return new InvalidVersionStringError(str, e);
            }
        });

    public record InvalidVersionStringError(string Value, Exception E)
        : Error($"Invalid version string. Actual value is: {Value}", E);
    
    // TODO: Delete this
    public CBORObject Encode()
    {
        var result = CBORObject.NewMap();

        var version = CBORObject.FromObject(Version.ToMajorMinorString());
        result.Add("version", version);

        var digestAlgorithm = CBORObject.FromObject(DigestAlgorithm.ToString());
        result.Add("digestAlgorithm", digestAlgorithm);

        var valueDigests = ValueDigests.ToCbor();
        result.Add("valueDigests", valueDigests);

        var docType = DocType.Encode();
        result.Add("docType", docType);

        var validityInfo = ValidityInfo.ToCbor();
        result.Add("validityInfo", validityInfo);

        var deviceKeyInfo = CBORObject.NewMap();
        
        var deviceKey = CBORObject.NewMap();
        var kty = CBORObject.FromObject(1);
        var ktyV = CBORObject.FromObject(2);
        deviceKey.Add(kty, ktyV);
        var crv = CBORObject.FromObject(-1);
        var crvV = CBORObject.FromObject(1);
        deviceKey.Add(crv, crvV);
        var x = CBORObject.FromObject(-2);
        var byteString = CBORObject.FromObject(Bla());
        deviceKey.Add(x, byteString);
        var y = CBORObject.FromObject(-3);
        var byteString2 = CBORObject.FromObject(Bla());
        deviceKey.Add(y, byteString2);
        deviceKeyInfo.Add("deviceKey", deviceKey);
        
        result.Add("deviceKeyInfo", deviceKeyInfo);

        return result;
    }
    
    public static byte[] Bla()
    {
        const int nonceLength = 32;
        var nonceBytes = new byte[nonceLength];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(nonceBytes);
        }

        return nonceBytes;
    }
}
