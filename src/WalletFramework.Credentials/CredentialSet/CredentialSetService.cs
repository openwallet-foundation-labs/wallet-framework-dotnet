using WalletFramework.Core.Credentials;
using WalletFramework.Core.StatusList;
using WalletFramework.Credentials.CredentialSet.Models;
using WalletFramework.Credentials.CredentialSet.Persistence;
using WalletFramework.Storage;

namespace WalletFramework.Credentials.CredentialSet;

public class CredentialSetService(
    ICredentialDataSetStore credentialDataSetStore,
    IStorageSession storageSession,
    IStatusListService statusListService) : ICredentialSetService
{
    public async Task<CredentialDataSet> RefreshCredentialSetState(CredentialDataSet credentialDataSet)
    {
        var (refreshedCredentialDataSet, hasChanged) = await RefreshState(credentialDataSet);
        if (hasChanged)
        {
            await storageSession.Commit();
        }

        return refreshedCredentialDataSet;
    }

    public async Task RefreshCredentialSetStates()
    {
        var credentialSetRecords = await credentialDataSetStore.List();
        var hasChanges = false;

        foreach (var credentialSetRecord in credentialSetRecords)
        {
            var (_, recordHasChanged) = await RefreshState(credentialSetRecord);
            hasChanges = hasChanges || recordHasChanged;
        }

        if (hasChanges)
        {
            await storageSession.Commit();
        }
    }

    private async Task<(CredentialDataSet CredentialDataSet, bool HasChanged)> RefreshState(
        CredentialDataSet credentialDataSet)
    {
        var oldState = credentialDataSet.State;

        if (credentialDataSet.DeletedAt.IsSome)
        {
            return (credentialDataSet, false);
        }

        credentialDataSet.ExpiresAt.IfSome(expiresAt =>
        {
            if (expiresAt < DateTime.UtcNow)
            {
                credentialDataSet = credentialDataSet with { State = CredentialState.Expired };
            }
        });

        await credentialDataSet.StatusListEntry.IfSomeAsync(async statusList =>
        {
            await statusListService.GetState(statusList).IfSomeAsync(state =>
            {
                if (state == CredentialState.Revoked)
                {
                    credentialDataSet = credentialDataSet with { State = CredentialState.Revoked };
                }
            });
        });

        var hasChanged = oldState != credentialDataSet.State;
        if (hasChanged)
        {
            await credentialDataSetStore.Save(credentialDataSet);
        }

        return (credentialDataSet, hasChanged);
    }
}
