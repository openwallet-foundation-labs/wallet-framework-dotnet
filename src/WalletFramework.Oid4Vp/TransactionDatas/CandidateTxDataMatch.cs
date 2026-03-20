using WalletFramework.Oid4Vp.Models;

namespace WalletFramework.Oid4Vp.TransactionDatas;

public record CandidateTxDataMatch(PresentationCandidate Candidate, TransactionData TransactionData)
{
    public string GetIdentifier()
    {
        return Candidate.Identifier;
    }
}
