using Hyperledger.Aries.Storage;
using LanguageExt;

namespace WalletFramework.Oid4Vc.Database.Migration;

public record MigrationStep(
    int OldVersion,
    int NewVersion,
    Type RecordType,
    Func<IEnumerable<RecordBase>, Task> Execute,
    Func<Task<Option<IEnumerable<RecordBase>>>> GetPendingRecords);
