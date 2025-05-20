namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

public struct CredentialIdentifier
{
    private string Value { get; }
    
    public override string ToString() => Value;
    
    public CredentialIdentifier(string id) => Value = id;
}
