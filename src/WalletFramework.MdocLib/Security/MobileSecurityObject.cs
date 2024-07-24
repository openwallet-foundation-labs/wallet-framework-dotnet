using PeterO.Cbor;
using WalletFramework.Core.Functional;
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

    // TODO: mdoc authentication
    // public DeviceKeyInfo DeviceKeyInfo { get; }

    public DocType DocType { get; }

    public ValidityInfo ValidityInfo { get; }

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
}
