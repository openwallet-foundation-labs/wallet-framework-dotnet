using FluentAssertions;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vci.CredConfiguration.Models;
using Xunit;

namespace WalletFramework.Oid4Vci.Tests.CredConfiguration;

public class ClaimMetadataTests
{
    [Fact]
    public void Claim_metadata_preserves_array_wildcard_paths()
    {
        var path = new JArray("credentialContent", "degreePrograms", JValue.CreateNull(), "name");
        var json = new JObject { ["path"] = path };

        var result = ClaimMetadata.ValidClaimMetadata(
            json,
            jt => jt.ToJArray().OnSuccess(ClaimPath.FromJArray));

        result.IsSuccess.Should().BeTrue();

        var roundTripped = result.UnwrapOrThrow().EncodeToJson()["path"]!;
        JToken.DeepEquals(roundTripped, path).Should().BeTrue();
    }
}
