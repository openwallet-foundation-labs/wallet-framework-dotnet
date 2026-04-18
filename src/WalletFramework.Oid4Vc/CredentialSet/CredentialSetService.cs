using WalletFramework.Core.Credentials;
using WalletFramework.Core.StatusList;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.Oid4Vc.CredentialSet.Persistence;
using WalletFramework.Storage;

namespace WalletFramework.Oid4Vc.CredentialSet;

public class CredentialSetService(
    IDomainRepository<CredentialDataSet, CredentialDataSetRecord, CredentialSetId> credentialSetRepository,
    IStatusListService statusListService) : ICredentialSetService
{
    public async Task<CredentialDataSet> RefreshCredentialSetState(CredentialDataSet credentialDataSet)
    {
        var oldState = credentialDataSet.State;

        if (credentialDataSet.DeletedAt.IsSome)
            return credentialDataSet;

        credentialDataSet.ExpiresAt.IfSome(expiresAt => 
        {
            if (expiresAt < DateTime.UtcNow)
            {
                credentialDataSet = credentialDataSet with { State = CredentialState.Expired };
            }
        });

        await credentialDataSet.StatusListEntry.IfSomeAsync(
            async statusList =>
            {
                await statusListService.GetState(statusList).IfSomeAsync(
                    state =>
                    {
                        if (state == CredentialState.Revoked)
                            credentialDataSet = credentialDataSet with { State = CredentialState.Revoked };
                    });
            });

        if (oldState != credentialDataSet.State)
            await credentialSetRepository.Update(credentialDataSet);

        return credentialDataSet;
    }

    public async Task RefreshCredentialSetStates()
    {
        var credentialSetRecords = await credentialSetRepository.ListAll();

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
