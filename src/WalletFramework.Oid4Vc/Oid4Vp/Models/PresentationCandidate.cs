using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;
using WalletFramework.Oid4Vc.Qes.Authorization;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///     Represents a list of credential candidates.
/// </summary>
public record PresentationCandidate
{
    /// <summary>
    ///     Gets a value indicating whether disclosures should be limited.
    /// </summary>
    public bool LimitDisclosuresRequired { get; }

    /// <summary>
    ///     Gets the array of credentials matching the request.
    /// </summary>
    public CredentialSetCandidate[] CredentialSetCandidates { get; }

    /// <summary>
    ///     Gets the Identifier of the candidate.
    /// </summary>
    public string Identifier { get; }

    public Option<List<ClaimQuery>> ClaimsToDisclose { get; init; }
    
    public Option<List<TransactionData>> TransactionData { get; init; } = 
        Option<List<TransactionData>>.None;
    
    public Option<List<Uc5QesTransactionData>> Uc5TransactionData { get; init; }
        = Option<List<Uc5QesTransactionData>>.None;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PresentationCandidate" /> class.
    /// </summary>
    /// <param name="identifier">The ID of the candidate.</param>
    /// <param name="credentialSets">The credentials matching the request.</param>
    /// <param name="claimsToDisclose">The claims to disclose.</param>
    /// <param name="limitDisclosuresRequired">Specifies whether disclosures should be limited.</param>
    public PresentationCandidate(
        string identifier,
        IEnumerable<CredentialSetCandidate> credentialSets,
        Option<List<ClaimQuery>> claimsToDisclose,
        bool limitDisclosuresRequired = false)
    {
        Identifier = identifier;
        CredentialSetCandidates = credentialSets.ToArray();
        LimitDisclosuresRequired = limitDisclosuresRequired;
        ClaimsToDisclose = claimsToDisclose;
    }
    
    public PresentationCandidate(
        string identifier,
        IEnumerable<CredentialSetCandidate> credentialSets,
        bool limitDisclosuresRequired = false)
    {
        Identifier = identifier;
        CredentialSetCandidates = credentialSets.ToArray();
        LimitDisclosuresRequired = limitDisclosuresRequired;
    }
}

public static class PresentationCandidateFun
{
    public static PresentationCandidate AddTransactionDatas(
        this PresentationCandidate candidate,
        IEnumerable<TransactionData> transactionDatas)
    {
        var td = candidate.TransactionData.Match(
            list => list.Append(transactionDatas),
            transactionDatas.ToList);

        return candidate with
        {
            TransactionData = td.ToList()
        };
    }
    
    public static PresentationCandidate AddUc5TransactionData(
        this PresentationCandidate candidate,
        IEnumerable<Uc5QesTransactionData> transactionData)
    {
        var td = candidate.Uc5TransactionData.Match(
            list => list.Append(transactionData),
            transactionData.ToList);

        return candidate with
        {
            Uc5TransactionData = td.ToList()
        };
    }
}
