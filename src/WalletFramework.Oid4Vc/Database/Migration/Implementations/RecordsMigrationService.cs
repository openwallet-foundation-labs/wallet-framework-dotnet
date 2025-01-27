using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using WalletFramework.Oid4Vc.Database.Migration.Abstraction;

namespace WalletFramework.Oid4Vc.Database.Migration.Implementations;

public class RecordsMigrationService : IRecordsMigrationService
{
    private readonly IAgentProvider _agentProvider;
    private readonly IWalletRecordService _walletRecordService;

    public RecordsMigrationService(
        IMigrationStepsProvider migrationStepsProvider,
        IAgentProvider agentProvider,
        IWalletRecordService walletRecordService)
    {
        _agentProvider = agentProvider;
        _walletRecordService = walletRecordService;
        
        var steps = migrationStepsProvider.Get();
        _walletFrameworkMigrationSteps.AddRange(steps);
    }

    private readonly List<MigrationStep> _migrationSteps = [];
    private readonly List<MigrationStep> _walletFrameworkMigrationSteps = [];

    public List<MigrationStep> MigrationSteps => 
        _walletFrameworkMigrationSteps
            .Append(_migrationSteps)
            .ToList();

    public async Task Migrate()
    {
        var stepGroups = MigrationSteps.GroupBy(step => new { step.RecordType, step.OldVersion, step.NewVersion }).OrderBy(x => x.Key.OldVersion).ThenBy(x => x.Key.NewVersion);
        foreach (var stepGroup in stepGroups)
        {
            var migratedRecords = new List<RecordBase>();
            
            foreach (var step in stepGroup)
            {
                var stepPendingRecords = await step.GetPendingRecords();
                await stepPendingRecords.IfSomeAsync(async pendingRecords =>
                {
                    var records = pendingRecords.ToList();
                    
                    await step.Execute(records);
                    migratedRecords.AddRange(records);
                });
            }

            var context = await _agentProvider.GetContextAsync();
            foreach (var record in migratedRecords)
            {
                record.RecordVersion = stepGroup.Key.NewVersion;
                await _walletRecordService.UpdateAsync(context.Wallet, record);
            }
        }
    }

    public IEnumerable<MigrationStep> AddMigrationStep(MigrationStep migrationStep)
    {
        _migrationSteps.Add(migrationStep);
        return MigrationSteps;
    }

    public IEnumerable<MigrationStep> AddMigrationSteps(IEnumerable<MigrationStep> steps)
    {
        _migrationSteps.AddRange(steps);
        return MigrationSteps;
    }

    public IEnumerable<MigrationStep> GetMigrationSteps() => MigrationSteps;
}
