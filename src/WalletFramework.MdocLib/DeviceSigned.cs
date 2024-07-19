namespace WalletFramework.MdocLib;

// TODO: mdoc authentication
public readonly struct DeviceSigned
{
    public NameSpaces NameSpaces { get; }
    
    public DeviceAuth DeviceAuth { get; }
}

public readonly struct DeviceAuth
{
    public DeviceSignature DeviceSignature { get; }
}

// TODO: Create COSE_Sign1 type
// This is a COSE_Sign1 object like in IssuerAuth
public readonly struct DeviceSignature
{
    public ProtectedHeaders ProtectedHeaders { get; }

    public UnprotectedHeaders UnprotectedHeaders { get; }

    public MobileSecurityObject Payload { get; }

    public CoseSignature Signature { get; }
}
