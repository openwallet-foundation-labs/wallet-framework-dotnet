using System.Collections.Immutable;
using FluentAssertions;
using Hyperledger.Aries.Storage;
using Hyperledger.Aries.Tests.Extensions;
using Moq;
using SD_JWT;
using WalletFramework.SdJwtVc.KeyStore.Services;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.SdJwtVc.Tests
{
    public class PexServiceTests
    {
        [Fact]
        public async Task Can_Create_Sd_Jwt_Presentation_Without_Key_Binding()
        {
            // Arrange
            var credential = new SdJwtRecord();

            const string issuerSignedJwt = "eyJhbGciOiAiRVMyNTYifQ.eyJfc2QiOiBbIkNyUWU3UzVrcUJBSHQtbk1ZWGdjNmJkdDJTSDVhVFkxc1VfTS1QZ2tqUEkiLCAiSnpZakg0c3ZsaUgwUjNQeUVNZmVadTZKdDY5dTVxZWhabzdGN0VQWWxTRSIsICJQb3JGYnBLdVZ1Nnh5bUphZ3ZrRnNGWEFiUm9jMkpHbEFVQTJCQTRvN2NJIiwgIlRHZjRvTGJnd2Q1SlFhSHlLVlFaVTlVZEdFMHc1cnREc3JaemZVYW9tTG8iLCAiWFFfM2tQS3QxWHlYN0tBTmtxVlI2eVoyVmE1TnJQSXZQWWJ5TXZSS0JNTSIsICJYekZyendzY002R242Q0pEYzZ2Vks4QmtNbmZHOHZPU0tmcFBJWmRBZmRFIiwgImdiT3NJNEVkcTJ4Mkt3LXc1d1BFemFrb2I5aFYxY1JEMEFUTjNvUUw5Sk0iLCAianN1OXlWdWx3UVFsaEZsTV8zSmx6TWFTRnpnbGhRRzBEcGZheVF3TFVLNCJdLCAiaXNzIjogImh0dHBzOi8vZXhhbXBsZS5jb20vaXNzdWVyIiwgImlhdCI6IDE2ODMwMDAwMDAsICJleHAiOiAxODgzMDAwMDAwLCAic3ViIjogInVzZXJfNDIiLCAibmF0aW9uYWxpdGllcyI6IFt7Ii4uLiI6ICJwRm5kamtaX1ZDem15VGE2VWpsWm8zZGgta284YUlLUWM5RGxHemhhVllvIn0sIHsiLi4uIjogIjdDZjZKa1B1ZHJ5M2xjYndIZ2VaOGtoQXYxVTFPU2xlclAwVmtCSnJXWjAifV0sICJfc2RfYWxnIjogInNoYS0yNTYiLCAiY25mIjogeyJqd2siOiB7Imt0eSI6ICJFQyIsICJjcnYiOiAiUC0yNTYiLCAieCI6ICJUQ0FFUjE5WnZ1M09IRjRqNFc0dmZTVm9ISVAxSUxpbERsczd2Q2VHZW1jIiwgInkiOiAiWnhqaVdXYlpNUUdIVldLVlE0aGJTSWlyc1ZmdWVjQ0U2dDRqVDlGMkhaUSJ9fX0.kmx687kUBiIDvKWgo2Dub-TpdCCRLZwtD7TOj4RoLsUbtFBI8sMrtH2BejXtm_P6fOAjKAVc_7LRNJFgm3PJhg";
            const string givenNameDisclosure = "WyIyR0xDNDJzS1F2ZUNmR2ZyeU5STjl3IiwgImdpdmVuX25hbWUiLCAiSm9obiJd";
            const string familyNameDisclosure = "WyJlbHVWNU9nM2dTTklJOEVZbnN4QV9BIiwgImZhbWlseV9uYW1lIiwgIkRvZSJd";
            const string nationalityDisclosure = "WyJsa2x4RjVqTVlsR1RQVW92TU5JdkNBIiwgIm5hdGlvbmFsaXR5IiwgImdlcm1hbiJd"; 
            
            credential.PrivateSet(x => x.EncodedIssuerSignedJwt, issuerSignedJwt);
            credential.PrivateSet(x => x.Disclosures, ImmutableArray.Create<string>(nationalityDisclosure, familyNameDisclosure, givenNameDisclosure));
            
            var claimsToDisclose = new[] { "given_name", "family_name" };

            const string expected = issuerSignedJwt + "~" + familyNameDisclosure + "~" + givenNameDisclosure + "~";
            
            var service = CreateSdJwtVcHolderService();

            // Act
            var presentation = await service.CreatePresentation(credential, claimsToDisclose);
            
            // Assert
            presentation.Should().BeEquivalentTo(expected);
        }

        private static SdJwtRecord CreateCredential(Dictionary<string, string> claims)
        {
            var record = new SdJwtRecord
            {
                Id = Guid.NewGuid().ToString(),
                Claims = claims
            };

            return record;
        }

        private static ISdJwtVcHolderService CreateSdJwtVcHolderService()
        {
            var holder = new Holder();
            var keystore = new MockKeystore();
            var walletRecordService = new Mock<IWalletRecordService>();

            return new DefaultSdJwtVcHolderService(holder, keystore, walletRecordService.Object);
        }

        private class MockKeystore : IKeyStore
        {
            public Task<string> GenerateKey(string alg = "ES256")
            {
                throw new NotImplementedException();
            }

            public Task<string> GenerateKbProofOfPossessionAsync(string keyId, string audience, string nonce, string type, string? sdHash = null)
            {
                throw new NotImplementedException();
            }

            public Task DeleteKey(string keyId)
            {
                throw new NotImplementedException();
            }

            public Task<string> GenerateKbProofOfPossessionAsync(string keyId, string audience, string nonce, string type)
            {
                throw new NotImplementedException();
            }

            public Task<string> GenerateDPopProofOfPossessionAsync(string keyId, string audience, string? nonce, string? accessToken)
            {
                throw new NotImplementedException();
            }

            public Task<string> LoadKey(string keyId)
            {
                throw new NotImplementedException();
            }

            public Task<byte[]> Sign(string keyId, byte[] payload)
            {
                throw new NotImplementedException();
            }
        }
    }
}
