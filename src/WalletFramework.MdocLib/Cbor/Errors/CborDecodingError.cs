using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Cbor.Errors;

public record CborDecodingError(Exception E) : Error(
    "The bytes could not be decoded with CBOR", E);
