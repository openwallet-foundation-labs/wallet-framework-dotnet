using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialSets;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.CredentialSets;

public class CredentialSetOptionTests
{
    private const string ValidJson = "[\"id1\", \"id2\", \"id3\"]";
    private const string InvalidJson = "[\"id1\", null, \"\"]";

    [Fact]
    public void Can_Parse_From_Json()
    {
        var array = JArray.Parse(ValidJson);

        var result = CredentialSetOption.FromJArray(array);

        result.Match(
            option => Assert.Equal(["id1", "id2", "id3"], option.Ids.Select(id => id.AsString())),
            errors => Assert.Fail($"Expected success but got errors: {string.Join(", ", errors.Select(e => e.Message))}")
        );
    }

    [Fact]
    public void Invalid_Json_Wont_Be_Parsed()
    {
        var array = JArray.Parse(InvalidJson);

        var result = CredentialSetOption.FromJArray(array);

        Assert.True(result.IsFail);
    }
} 
