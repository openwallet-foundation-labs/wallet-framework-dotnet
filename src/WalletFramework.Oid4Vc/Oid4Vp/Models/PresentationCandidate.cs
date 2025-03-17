using LanguageExt;
using WalletFramework.Oid4Vc.Payment;

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
    
    public Option<List<PaymentTransactionData>> TransactionData { get; init; } = 
        Option<List<PaymentTransactionData>>.None;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PresentationCandidate" /> class.
    /// </summary>
    /// <param name="identifier">The ID of the candidate.</param>
    /// <param name="credentialSets">The credentials matching the request.</param>
    /// <param name="limitDisclosuresRequired">Specifies whether disclosures should be limited.</param>
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
    public static PresentationCandidate AddTransactionData(
        this PresentationCandidate candidate,
        PaymentTransactionData transactionData)
    {
        var td = candidate.TransactionData.Match(
            list => list.Append(transactionData),
            () => [transactionData]);

        return candidate with
        {
            TransactionData = td.ToList()
        };
    }
}
