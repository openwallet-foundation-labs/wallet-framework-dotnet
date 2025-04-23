namespace WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Models;

public readonly struct CNonceEndpoint
{
    public Uri Value { get; }
    
    public CNonceEndpoint(Uri endpoint)
    {
        Value = endpoint;
    }
};
