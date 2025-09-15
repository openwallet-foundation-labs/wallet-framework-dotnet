namespace WalletFramework.Oid4Vc.Database.Migration.Abstraction;

public interface IMigrationStepsProvider
{
    public IEnumerable<MigrationStep> Get();
}
