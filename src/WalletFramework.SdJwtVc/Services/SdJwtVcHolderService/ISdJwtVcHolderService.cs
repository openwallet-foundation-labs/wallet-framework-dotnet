using LanguageExt;
using WalletFramework.Core.ClaimPaths;

namespace WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

/// <summary>
///     Provides methods for handling SD-JWT credentials.
/// </summary>
public interface ISdJwtVcHolderService
{
    /// <summary>
    ///     Creates a SD-JWT in presentation format where the provided claims are disclosed.
    ///     The key binding is optional and can be activated by providing an audience and a nonce.
    /// </summary>
    /// <param name="disclosedClaimPaths">The claim paths to disclose</param>
    /// <param name="sdJwt">The SD-JWT credential</param>
    /// <param name="transactionDataHashes">The transaction data hashes</param>
    /// <param name="transactionDataHashesAlg">The transaction data hashes alg</param>
    /// <param name="audience">The targeted audience</param>
    /// <param name="nonce">The nonce</param>
    /// <returns>The SD-JWT in presentation format</returns>
    Task<string> CreatePresentation(
        SdJwtCredential sdJwt,
        ClaimPath[] disclosedClaimPaths,
        Option<IEnumerable<string>> transactionDataHashes,
        Option<string> transactionDataHashesAlg,
        string? audience = null,
        string? nonce = null);
}
