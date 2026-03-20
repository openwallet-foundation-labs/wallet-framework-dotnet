namespace WalletFramework.Oid4Vci.Authorization.DPop.Models;

internal record DPop
{
    public DPopConfig Config { get; }
    
    internal DPop(DPopConfig config)
    {
        Config = config;
    }
}
