using FluentAssertions;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;
using WalletFramework.SdJwtVc.Models.Records;

namespace WalletFramework.SdJwtVc.Tests;

public class SdJwtRecordTests
{
    [Fact]
    public void CanCreateSdJwtRecord()
    {
        var encodedSdJwt = Samples.EncodedSdJwt;
        var keyId = KeyId.CreateKeyId();
        
        var record = new SdJwtRecord(
            encodedSdJwt,
            new Dictionary<string, ClaimMetadata>(),
            new List<SdJwtDisplay>(),
            keyId,
            CredentialSetId.CreateCredentialSetId());
        
        record.Claims.Count.Should().Be(10);
    }
}
