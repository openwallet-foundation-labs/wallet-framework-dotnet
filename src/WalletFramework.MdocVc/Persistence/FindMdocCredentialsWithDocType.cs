using System.Linq.Expressions;
using WalletFramework.MdocLib;
using WalletFramework.Storage;

namespace WalletFramework.MdocVc.Persistence;

public record FindMdocCredentialsWithDocType : ISearchConfig<MdocCredentialRecord>
{
    public DocType DocType { get; set; }

    public Expression<Func<MdocCredentialRecord, bool>> ToPredicate()
    {
        return record => record.DocType == DocType;
    }
}
