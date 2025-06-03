using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;

public record ScopedCredentialConfiguration(CredentialConfigurationId CredentialConfigurationId, Option<Scope> Scope);

public static class ScopedCredentialConfigurationExtensions
{
    private const string ScopeJsonKey = "scope";
    private const string CredentialConfigurationIdJsonKey = "credential_configuration_id";

    public static JObject EncodeToJson(this ScopedCredentialConfiguration scopedCredentialConfiguration)
    {
        return new JObject
        {
            { ScopeJsonKey, scopedCredentialConfiguration.Scope.MatchUnsafe(scope => scope.ToString(), () => null) },
            { CredentialConfigurationIdJsonKey, scopedCredentialConfiguration.CredentialConfigurationId.ToString() }
        };
    }

    public static ScopedCredentialConfiguration DecodeFromJson(JObject json)
    {
        var scope = Scope.OptionalScope(json[ScopeJsonKey]!);
        var credentialConfigurationId = CredentialConfigurationId.ValidCredentialConfigurationId(json[CredentialConfigurationIdJsonKey]!.ToString()).UnwrapOrThrow();

        return new ScopedCredentialConfiguration(credentialConfigurationId, scope);
    }
}
