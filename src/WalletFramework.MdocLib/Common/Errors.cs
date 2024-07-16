using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Common;

public record InvalidCborByteStringError(string Name, Exception E)
    : Error($"The value of *{Name}* is not a valid CBOR object encoded as a byte string", E);

public record CborIsNotATextStringError(string Name, Exception E)
    : Error($"The value of *{Name}* is not a text string", E);

public record CborValueIsNullOrEmptyError(string Name) : Error($"The value of *{Name}* is null or empty");

public record CborFieldNotFoundError(string Name) : Error($"The field *{Name}* was not found");

public record InvalidDateTimeStringError(string Name, Exception E)
    : Error($"The value of *{Name}* is not a valid DateTime string", E);

public record CborIsNotAByteStringError(string Name, Exception E)
    : Error($"The value of *{Name}* is not byte String", E);

public record CborIsNotAMapOrAnArrayError : Error
{
    public CborIsNotAMapOrAnArrayError(
        string cborStr,
        string label,
        Exception e) : base(
        $"Cant find or access *{label}* in CBOR because its not a map or an array. Actual value is: {cborStr}", e)
    {
    }

    public CborIsNotAMapOrAnArrayError(
        string cborStr,
        Exception e) : base($"Cant access item in CBOR because is not a map or an array. Actual value is: {cborStr}", e)
    {
    }
}

public record IndexOutsideOfCborBoundsError(string CborStr, uint Index)
    : Error($"Cant access index *{Index}* in CBOR because its outside of bounds. Cbor is: {CborStr}");

public record InvalidDigestError(DigestId Id, string ComputedDigest, string ActualDigest)
    : Error($"The computed digest: *{ComputedDigest}* with ID *{Id}* is not equal to the actual digest *{ActualDigest}*");

public record DocTypeInMdocDoesNotMatchWithDocTypeInMsoError(DocType mdocDocType, DocType msoDocType)
    : Error($"The DocType in the mdoc: *{mdocDocType}* does not match with the DocType in the MSO: *{msoDocType}*");

public record FieldValueIsNullOrEmptyError(string name) : Error($"The value of the field *{name}* is null or empty");

public record NameSpaceIsNullOrEmptyError() : Error("The value of a namespace is null or empty");

public record ElementIdentifierIsNullOrEmptyError() : Error("The value of an element identifier is null or empty");
