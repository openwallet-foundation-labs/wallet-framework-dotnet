using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;
using WalletFramework.Oid4Vc.Qes;

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
    ///     Gets the array of credentials matching the input descriptor.
    /// </summary>
    public CredentialSetCandidate[] CredentialSetCandidates { get; }

    /// <summary>
    ///     Gets the ID of the input descriptor.
    /// </summary>
    public string InputDescriptorId { get; }
    
    public Option<List<TransactionData>> TransactionData { get; init; } = 
        Option<List<TransactionData>>.None;
    
    public Option<List<Uc5QesTransactionData>> Uc5TransactionData { get; init; }
        = Option<List<Uc5QesTransactionData>>.None;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PresentationCandidate" /> class.
    /// </summary>
    /// <param name="inputDescriptorId">The ID of the input descriptor.</param>
    /// <param name="credentialSets">The credentials matching the input descriptor.</param>
    /// <param name="limitDisclosuresRequired">Specifies whether disclosures should be limited.</param>
    public PresentationCandidate(
        string inputDescriptorId,
        IEnumerable<CredentialSetCandidate> credentialSets,
        bool limitDisclosuresRequired = false)
    {
        InputDescriptorId = inputDescriptorId;
        CredentialSetCandidates = credentialSets.ToArray();
        LimitDisclosuresRequired = limitDisclosuresRequired;
    }
}

public static class PresentationCandidateFun
{
    public static PresentationCandidate AddTransactionData(
        this PresentationCandidate candidate,
        TransactionData transactionData)
    {
        var td = candidate.TransactionData.Match(
            list => list.Append(transactionData),
            () => [transactionData]);

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
