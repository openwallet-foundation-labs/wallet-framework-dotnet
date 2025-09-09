namespace WalletFramework.SdJwtLib.Roles;

public interface IVerifier
{
    public bool VerifyPresentation(string presentation, string issuerJwk);
}

