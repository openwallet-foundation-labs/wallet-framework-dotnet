using WalletFramework.SdJwtVc.KeyStore.Services;

namespace WalletFramework.SdJwtVc.Tests
{
    public class SdJwtVcHolderTests
    {
        // Todo: Implement tests
        private class MockKeystore : IKeyStore
        {
            public Task<string> GenerateKey(string alg = "ES256")
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

            public Task<string> GenerateProofOfPossessionAsync(string keyId, string audience, string nonce, string type)
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

            public Task DeleteKey(string keyId)
            {
                throw new NotImplementedException();
            }
        }
    }
}
