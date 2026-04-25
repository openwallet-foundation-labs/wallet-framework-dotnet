namespace WalletFramework.Oid4Vci.CredentialNonce.Models;

public readonly struct CredentialNonceEndpoint(Uri endpoint)
{
    public Uri Value { get; } = endpoint;
};
