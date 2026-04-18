using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.MdocLib;
using WalletFramework.Storage.Repositories;

namespace WalletFramework.MdocVc.Persistence;

public class MdocCredentialRepository(IRepository<MdocCredentialRecord> repository)
    : IMdocCredentialStore
{
    public async Task<Unit> Add(MdocCredential credential)
    {
        var record = new MdocCredentialRecord(credential);
        await repository.Add(record);
        return Unit.Default;
    }

    public async Task<Option<MdocCredential>> Get(CredentialId id)
    {
        var guid = Guid.Parse(id.AsString());
        var record = await repository.GetById(guid);
        return record.Map(item => item.ToDomainModel());
    }

    public async Task<IReadOnlyList<MdocCredential>> List()
    {
        var records = await repository.ListAll();
        return MapRecords(records);
    }

    public async Task<IReadOnlyList<MdocCredential>> ListByDocType(DocType docType)
    {
        var docTypeValue = docType.AsString();
        var records = await repository.Find(record => record.DocType == docTypeValue);
        return MapRecords(records);
    }

    public async Task<Unit> Update(MdocCredential credential)
    {
        var record = new MdocCredentialRecord(credential);
        await repository.Update(record);
        return Unit.Default;
    }

    public async Task<Unit> Delete(CredentialId id)
    {
        var guid = Guid.Parse(id.AsString());
        await repository.RemoveById(guid);
        return Unit.Default;
    }

    private static IReadOnlyList<MdocCredential> MapRecords(Option<IReadOnlyList<MdocCredentialRecord>> records)
    {
        return records.Match<IReadOnlyList<MdocCredential>>(
            credentialRecords => credentialRecords.Select(record => record.ToDomainModel()).ToList(),
            () => []);
    }
}
