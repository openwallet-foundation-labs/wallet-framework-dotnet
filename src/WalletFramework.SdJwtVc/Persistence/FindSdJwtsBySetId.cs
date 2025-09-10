using System.Linq.Expressions;
using WalletFramework.Core.Credentials;
using WalletFramework.Storage;

namespace WalletFramework.SdJwtVc.Persistence;

public sealed record FindSdJwtsBySetId(CredentialSetId CredentialSetId)
    : ISearchConfig<SdJwtCredentialRecord>
{
    public Expression<Func<SdJwtCredentialRecord, bool>> ToPredicate()
    {
        var setId = CredentialSetId.AsString();
        return r => r.CredentialSetId == setId;
    }
}
