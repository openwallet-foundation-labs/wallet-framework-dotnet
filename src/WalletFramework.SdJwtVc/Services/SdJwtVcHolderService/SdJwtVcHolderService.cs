using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using LanguageExt;
using SD_JWT.Models;
using SD_JWT.Roles;
using WalletFramework.Core.Credentials;
using WalletFramework.SdJwtVc.Models.Records;

namespace WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

/// <inheritdoc />
public class SdJwtVcHolderService(
    IHolder holder,
    ISdJwtSigner signer,
    IWalletRecordService recordService) : ISdJwtVcHolderService
{
    /// <inheritdoc />
    public virtual async Task AddAsync(IAgentContext context, SdJwtRecord record) =>
        await recordService.AddAsync(context.Wallet, record);

    /// <inheritdoc />
    public async Task<string> CreatePresentation(
        SdJwtRecord credential,
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

    /// <inheritdoc />
    public virtual async Task<bool> DeleteAsync(IAgentContext context, string recordId) =>
        await recordService.DeleteAsync<SdJwtRecord>(context.Wallet, recordId);

    /// <inheritdoc />
    public async Task<SdJwtRecord> GetAsync(IAgentContext context, string credentialId)
    {
        var record = await recordService.GetAsync<SdJwtRecord>(context.Wallet, credentialId);
        if (record == null)
            throw new AriesFrameworkException(ErrorCode.RecordNotFound, "SD-JWT Credential record not found");

        return record;
    }

    /// <inheritdoc />
    public Task<List<SdJwtRecord>> ListAsync(
        IAgentContext context,
        ISearchQuery? query = null,
        int count = 100,
        int skip = 0) => recordService.SearchAsync<SdJwtRecord>(context.Wallet, query, null, count, skip);

    public async Task<Option<IEnumerable<SdJwtRecord>>> ListAsync(IAgentContext context, CredentialSetId id)
    {
        var sdJwtQuery = SearchQuery.Equal(
            "~" + nameof(SdJwtRecord.CredentialSetId),
            id);

        var sdJwtRecords = await ListAsync(
            context,
            sdJwtQuery);

        return sdJwtRecords.Any()
            ? sdJwtRecords
            : Option<IEnumerable<SdJwtRecord>>.None;
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(IAgentContext context, SdJwtRecord record) =>
        await recordService.UpdateAsync(context.Wallet, record);
}

internal static class SdJwtRecordExtensions
{
    internal static SdJwtDoc ToSdJwtDoc(this SdJwtRecord record) =>
        new(record.EncodedIssuerSignedJwt + "~" + string.Join("~", record.Disclosures) + "~");
}
