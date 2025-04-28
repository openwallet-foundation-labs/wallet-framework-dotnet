namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

public readonly struct PrivacyPolicyUri
{
    public Uri Value { get; }
    
    private PrivacyPolicyUri(Uri value) => Value = value;
    
    public static implicit operator string(PrivacyPolicyUri policyUri) => policyUri.Value.ToString();
    
    public static implicit operator Uri(PrivacyPolicyUri policyUri) => policyUri.Value;
    
    public static implicit operator PrivacyPolicyUri(Uri uri) => CreatePrivacyPolicyUri(uri.ToString());
    
    public static implicit operator PrivacyPolicyUri(string uri) => CreatePrivacyPolicyUri(uri);
    
    public override string ToString() => Value.ToString();
    
    public static PrivacyPolicyUri CreatePrivacyPolicyUri(string uri)
    {
        try
        {
            return new PrivacyPolicyUri(new Uri(uri));
        }
        catch (Exception e)
        {
            throw new ArgumentException(e.Message, nameof(uri));
        }
    }
}
