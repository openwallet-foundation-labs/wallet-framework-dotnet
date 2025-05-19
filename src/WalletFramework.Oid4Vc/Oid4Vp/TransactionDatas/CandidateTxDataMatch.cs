using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

public record CandidateTxDataMatch(PresentationCandidate Candidate, TransactionData TransactionData)
{
    public string GetIdentifier()
    {
        return Candidate.Identifier;
    }
}
