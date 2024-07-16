namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;

internal readonly struct DPopNonce
{
    private string Value { get; }
    
    public static implicit operator string(DPopNonce nonce) => nonce.Value;
    
    internal DPopNonce(string nonce)
    {
        if (string.IsNullOrEmpty(nonce))
        {
            throw new InvalidOperationException("nonce must not be null or empty");
        }
        
        Value = nonce;
    }
}
