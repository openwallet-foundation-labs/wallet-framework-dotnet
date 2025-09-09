namespace WalletFramework.Oid4Vc.Oid4Vp.Errors;

public record VpFormatsNotSupportedError : VpError
{
    private const string Code = "vp_formats_not_supported";

    public VpFormatsNotSupportedError(string message) : base(Code, message)
    {
    }
};
