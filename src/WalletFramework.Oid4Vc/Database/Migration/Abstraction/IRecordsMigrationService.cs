namespace WalletFramework.Oid4Vc.Database.Migration.Abstraction;

public interface IRecordsMigrationService
{
    public Task Migrate();
    
    public IEnumerable<MigrationStep> AddMigrationStep(MigrationStep migrationStep);

    public IEnumerable<MigrationStep> AddMigrationSteps(IEnumerable<MigrationStep> steps);

    public IEnumerable<MigrationStep> GetMigrationSteps();
}
