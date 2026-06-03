using FluentAssertions;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vci.Issuer.Models;
using Xunit;

namespace WalletFramework.Oid4Vci.Tests.Issuer;

public class CredentialIssuerIdTests
{
    [Fact(DisplayName = "credential issuer identifiers preserve trailing slashes")]
    public void CredentialIssuerIdentifiersPreserveTrailingSlashes()
    {
        const string issuer = "https://test-issuer.com/test/a/sdjwtEncrypted/";

        var credentialIssuerId = CredentialIssuerId
            .ValidCredentialIssuerId(new JValue(issuer))
            .UnwrapOrThrow(new InvalidOperationException());
        Uri parsedUri = credentialIssuerId;

        credentialIssuerId.ToString().Should().Be(issuer);
        parsedUri.Should().Be(new Uri(issuer));
    }
}
