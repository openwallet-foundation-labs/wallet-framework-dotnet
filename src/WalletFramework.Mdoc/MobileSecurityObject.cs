using PeterO.Cbor;
using WalletFramework.Functional;
using WalletFramework.Mdoc.Common;
using static WalletFramework.Mdoc.DigestAlgorithm;
using static WalletFramework.Mdoc.DocType;
using static WalletFramework.Mdoc.ValidityInfo;
using static WalletFramework.Mdoc.CborByteString;
using static WalletFramework.Mdoc.ValueDigests;
using static WalletFramework.Functional.ValidationFun;

namespace WalletFramework.Mdoc;

public readonly struct MobileSecurityObject
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
        from mso in issuerAuth.GetByIndex(2)
        from byteString in ValidCborByteString(mso)
        let decoded = byteString.Decode()
        from result in Valid(Create)
            .Apply(byteString)
            .Apply(ValidMsoVersion(decoded))
            .Apply(ValidDigestAlgorithm(decoded))
            .Apply(decoded.GetByLabel("valueDigests").OnSuccess(ValidValueDigests))
            .Apply(ValidDoctype(decoded))
            .Apply(ValidValidityInfo(decoded))
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
