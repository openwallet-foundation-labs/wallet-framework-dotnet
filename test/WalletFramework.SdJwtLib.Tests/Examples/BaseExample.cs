namespace WalletFramework.SdJwtLib.Tests.Examples;

public abstract class BaseExample
{
    public abstract int NumberOfDisclosures { get; }
    
    public abstract string IssuedSdJwt { get; }
    
    public abstract string UnsecuredPayload { get; }
    
    public abstract string SecuredPayload { get; }
}
