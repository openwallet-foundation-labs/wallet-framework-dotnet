namespace WalletFramework.MdocLib.Device.Response;

public enum StatusCode
{
    Ok = 0,
    GeneralError = 10,
    CborDecodingError = 11,
    CborValidationError = 12
}
