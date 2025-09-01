using LanguageExt;

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
    /// <remarks>
    ///     The SD-JWT is created using the provided SD-JWT credential and the provided claims are disclosed
    /// </remarks>
    /// <param name="disclosedClaimPaths">The claims to disclose</param>
    /// <param name="sdJwt">The SD-JWT credential</param>
    /// <param name="transactionDataBase64UrlStrings">The transaction data base64 URL strings</param>
    /// <param name="transactionDataHashes">The transaction data hashes</param>
    /// <param name="transactionDataHashesAlg">The transaction data hashes alg</param>
    /// <param name="audience">The targeted audience</param>
    /// <param name="nonce">The nonce</param>
    /// <returns>The SD-JWT in presentation format</returns>
    Task<string> CreatePresentation(
        SdJwtCredential sdJwt,
        string[] disclosedClaimPaths,
        Option<IEnumerable<string>> transactionDataBase64UrlStrings,
        Option<IEnumerable<string>> transactionDataHashes,
        Option<string> transactionDataHashesAlg,
        string? audience = null,
        string? nonce = null);
}
