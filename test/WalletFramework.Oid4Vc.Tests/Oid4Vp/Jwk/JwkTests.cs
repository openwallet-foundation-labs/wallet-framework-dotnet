using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Jwk;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Jwk;

public class JwkTests
{
    [Fact]
    public void Can_Parse_Jwks()
    {
        var sample = "";

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
