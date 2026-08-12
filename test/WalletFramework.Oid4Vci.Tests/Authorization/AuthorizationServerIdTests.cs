using FluentAssertions;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vci.Authorization.Models;
using Xunit;

namespace WalletFramework.Oid4Vci.Tests.Authorization;

public class AuthorizationServerIdTests
{
    [Fact(DisplayName = "authorization server identifiers preserve trailing slashes")]
    public void AuthorizationServerIdentifiersPreserveTrailingSlashes()
    {
        const string issuer = "https://test-issuer.com/test/a/sdjwtEncrypted/";

        var authorizationServerId = AuthorizationServerId
            .ValidAuthorizationServerId(new JValue(issuer))
            .UnwrapOrThrow(new InvalidOperationException());
        Uri parsedUri = authorizationServerId;

        authorizationServerId.ToString().Should().Be(issuer);
        parsedUri.Should().Be(new Uri(issuer));
    }
}
