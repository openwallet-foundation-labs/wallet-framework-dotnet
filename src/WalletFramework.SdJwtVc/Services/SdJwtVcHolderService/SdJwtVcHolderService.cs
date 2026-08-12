using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Functional;
using WalletFramework.SdJwtLib.Models;
using WalletFramework.SdJwtLib.Roles;
using PathSet = System.Collections.Generic.HashSet<string>;

namespace WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

/// <inheritdoc />
public class SdJwtVcHolderService(IHolder holder, ISdJwtSigner signer) : ISdJwtVcHolderService
{
    /// <inheritdoc />
    public async Task<string> CreatePresentation(
        SdJwtCredential credential,
        ClaimPath[] disclosedClaimPaths,
        Option<IEnumerable<string>> transactionDataHashes,
        Option<string> transactionDataHashesAlg,
        string? audience = null,
        string? nonce = null)
    {
        var sdJwtDoc = credential.ToSdJwtDoc();

        var requiredPaths = CollectRequiredDisclosurePaths(sdJwtDoc.UnsecuredPayload, disclosedClaimPaths);

        var disclosures = sdJwtDoc
            .Disclosures
            .Where(disclosure => !string.IsNullOrEmpty(disclosure.Path) && requiredPaths.Contains(disclosure.Path!))
            .ToArray();

        var presentationFormat =
            holder.CreatePresentationFormat(credential.EncodedIssuerSignedJwt, disclosures);

        if (credential.KeyId.IsSome
            && !string.IsNullOrEmpty(nonce)
            && !string.IsNullOrEmpty(audience))
        {
            var keybindingJwt = await signer.GenerateKbProofOfPossessionAsync(
                credential.KeyId.UnwrapOrThrow(),
                audience,
                nonce,
                "kb+jwt",
                presentationFormat.ToSdHash(),
                null,
                transactionDataHashes,
                transactionDataHashesAlg);

            return presentationFormat.AddKeyBindingJwt(keybindingJwt);
        }

        return presentationFormat.Value;
    }

    private static PathSet CollectRequiredDisclosurePaths(
        JObject payload,
        IEnumerable<ClaimPath> paths)
    {
        var result = new PathSet();

        foreach (var path in paths)
        {
            path
                .ProcessWith(payload)
                .OnSuccess(selection =>
                {
                    foreach (var token in selection.GetValues())
                    foreach (var ancestorPath in AncestorPaths(token))
                        result.Add(ancestorPath);

                    return Unit.Default;
                });
        }

        return result;
    }

    private static IEnumerable<string> AncestorPaths(JToken token)
    {
        for (JToken? current = token; current is not null; current = current.Parent)
        {
            if (!string.IsNullOrEmpty(current.Path))
                yield return current.Path;
        }
    }
}
