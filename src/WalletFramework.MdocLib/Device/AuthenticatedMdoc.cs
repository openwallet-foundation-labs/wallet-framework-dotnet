namespace WalletFramework.MdocLib.Device;

public readonly struct AuthenticatedMdoc
{
    public Mdoc Mdoc { get; }
    
    public DeviceSigned DeviceSigned { get; }
    
    public AuthenticatedMdoc(Mdoc mdoc, DeviceSigned deviceSigned)
    {
        Mdoc = mdoc;
        DeviceSigned = deviceSigned;
    }
}
