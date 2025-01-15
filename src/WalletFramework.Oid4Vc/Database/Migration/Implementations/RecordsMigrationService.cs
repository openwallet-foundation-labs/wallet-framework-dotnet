using WalletFramework.Oid4Vc.Database.Migration.Abstraction;

namespace WalletFramework.Oid4Vc.Database.Migration.Implementations;

public class RecordsMigrationService : IRecordsMigrationService
{
    public RecordsMigrationService(IMigrationStepsProvider migrationStepsProvider)
    {
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
        var steps = _walletFrameworkMigrationSteps.Append(_migrationSteps);

        var migrations = steps.Select(async step =>
       {
            var pendingRecords = await step.GetPendingRecords();
            await pendingRecords.IfSomeAsync(async records =>
            {
                await step.Execute(records);
            });
        });

        await Task.WhenAll(migrations);
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
