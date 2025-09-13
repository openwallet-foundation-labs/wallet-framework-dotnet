using LanguageExt;
using WalletFramework.SdJwtLib.Models;
using WalletFramework.SdJwtLib.Roles;

namespace WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

/// <inheritdoc />
public class SdJwtVcHolderService(IHolder holder, ISdJwtSigner signer) : ISdJwtVcHolderService
{
    /// <inheritdoc />
    public async Task<string> CreatePresentation(
        SdJwtCredential credential,
        string[] disclosedClaimPaths,
        Option<IEnumerable<string>> transactionDataBase64UrlStrings,
        Option<IEnumerable<string>> transactionDataHashes,
        Option<string> transactionDataHashesAlg,
        string? audience = null,
        string? nonce = null)
    {
        var sdJwtDoc = credential.ToSdJwtDoc();
        var disclosures = new List<Disclosure>();
        foreach (var disclosure in sdJwtDoc.Disclosures)
        {
            if (disclosedClaimPaths.Any(disclosedClaim => disclosedClaim.StartsWith(disclosure.Path ?? string.Empty)))
            {
                disclosures.Add(disclosure);
            }
        }

        var presentationFormat =
            holder.CreatePresentationFormat(credential.EncodedIssuerSignedJwt, disclosures.ToArray());

        if (!string.IsNullOrEmpty(credential.KeyId)
            && !string.IsNullOrEmpty(nonce)
            && !string.IsNullOrEmpty(audience))
        {
            var keybindingJwt = await signer.GenerateKbProofOfPossessionAsync(
                credential.KeyId,
                audience,
                nonce,
                "kb+jwt",
                presentationFormat.ToSdHash(),
                null,
                transactionDataBase64UrlStrings,
                transactionDataHashes,
                transactionDataHashesAlg);

            return presentationFormat.AddKeyBindingJwt(keybindingJwt);
        }

        return presentationFormat.Value;
    }
}
