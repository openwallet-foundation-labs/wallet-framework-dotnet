using WalletFramework.Core.Encoding;

namespace WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

/// <summary>
///     The transaction data hash which will be put into the response
/// </summary>
/// <param name="hash">The hash</param>
/// <remarks>Currently only supports sha-256</remarks>
public readonly struct TransactionDataHash(Sha256Hash hash, TransactionDataHashesAlg alg)
{
    public TransactionDataHashesAlg Alg { get; } = alg;
    
    public string AsHex => hash.AsHex;
}    

public static class TransactionDataHashFun
{
    public static TransactionDataHash Hash(this TransactionData transactionData, TransactionDataHashesAlg alg)
    {
        var hashWith256 = new Func<TransactionData, TransactionDataHash>(data =>
        {
            var bytes = data.GetEncoded().AsByteArray;
            var hash = Sha256Hash.ComputeHash(bytes);
            return new TransactionDataHash(hash, TransactionDataHashesAlg.Sha256);
        });

        return alg.AsString switch
        {
            "sha-256" => hashWith256(transactionData),
            _ => throw new InvalidOperationException($"The transaction data hash alg {alg.AsString} is not supported")
        };
    }
}
