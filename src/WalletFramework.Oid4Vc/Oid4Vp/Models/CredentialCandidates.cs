using WalletFramework.Core.Credentials.Abstractions;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///     Represents a list of credential candidates.
/// </summary>
public class CredentialCandidates
{
    /// <summary>
    ///     Gets a value indicating whether disclosures should be limited.
    /// </summary>
    public bool LimitDisclosuresRequired { get; private set; }

    /// <summary>
    ///     Gets the array of credentials matching the input descriptor.
    /// </summary>
    public CredentialSetCandidate[] CredentialSetCandidates { get; private set; }

    /// <summary>
    ///     Gets the ID of the input descriptor.
    /// </summary>
    public string InputDescriptorId { get; private set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CredentialCandidates" /> class.
    /// </summary>
    /// <param name="inputDescriptorId">The ID of the input descriptor.</param>
    /// <param name="credentials">The credentials matching the input descriptor.</param>
    /// <param name="limitDisclosuresRequired">Specifies whether disclosures should be limited.</param>
    public CredentialCandidates(string inputDescriptorId, IEnumerable<CredentialSetCandidate> credentials,
        bool limitDisclosuresRequired = false)
    {
        InputDescriptorId = inputDescriptorId;
        CredentialSetCandidates = credentials.ToArray();
        LimitDisclosuresRequired = limitDisclosuresRequired;
    }
}
