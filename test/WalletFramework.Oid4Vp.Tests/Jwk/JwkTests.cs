using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.Jwk;
using WalletFramework.Oid4Vp.Tests.Jwk.Samples;

namespace WalletFramework.Oid4Vp.Tests.Jwk;

public class JwkTests
{
    [Fact]
    public void Can_Parse_Jwks()
    {
        var sample = JwkSamples.GetStrSample();

        var sut = JwkSet.FromJsonStr(sample);
        sut.Match(
            jwkSet =>
            {
                
            },
            errors =>
            {
                var folded = errors.Reduce(
                    (acc, error) => acc with
                    {
                        Message = acc.Message + error.Message
                    });
                
                Assert.Fail(folded.Message);
            }
        );
    }
}
