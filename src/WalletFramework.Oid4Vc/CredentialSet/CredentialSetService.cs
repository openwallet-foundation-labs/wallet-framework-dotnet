using Hyperledger.Aries.Storage;
using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.StatusList;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.SdJwtVc.Services;

namespace WalletFramework.Oid4Vc.CredentialSet;

public class CredentialSetService(
    ICredentialSetStorage storage,
    IStatusListService statusListService) : ICredentialSetService
{
    public async Task<CredentialSetRecord> RefreshCredentialSetState(CredentialSetRecord credentialSetRecord)
    {
        var oldState = credentialSetRecord.State;

        if (credentialSetRecord.IsDeleted())
            return credentialSetRecord;

        credentialSetRecord.ExpiresAt.IfSome(expiresAt =>
        {
            if (expiresAt < DateTime.UtcNow)
                credentialSetRecord.State = CredentialState.Expired;
        });

        await credentialSetRecord.StatusListEntry.IfSomeAsync(
            async statusList =>
            {
                await statusListService.GetState(statusList).IfSomeAsync(
                    state =>
                    {
                        if (state == CredentialState.Revoked)
                            credentialSetRecord.State = CredentialState.Revoked;
                    });
            });

        if (oldState != credentialSetRecord.State)
            await storage.Update(credentialSetRecord);

        return credentialSetRecord;
    }

    public async Task RefreshCredentialSetStates()
    {
        var credentialSetRecords = await storage.List(Option<ISearchQuery>.None);

        await credentialSetRecords.IfSomeAsync(
            async records =>
            {
                foreach (var credentialSetRecord in records)
                {
                    await RefreshCredentialSetState(credentialSetRecord);
                }
            });
    }
}
