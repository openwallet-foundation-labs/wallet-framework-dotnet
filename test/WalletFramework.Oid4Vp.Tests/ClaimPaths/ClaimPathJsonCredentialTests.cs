using FluentAssertions;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.ClaimPaths.Errors;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.ClaimPaths;
using static WalletFramework.Oid4Vp.Tests.ClaimPaths.Samples.ClaimPathSamples;

namespace WalletFramework.Oid4Vp.Tests.ClaimPaths;

public class ClaimPathJsonCredentialTests
{
    [Fact]
    public void Can_Process_Index_Components()
    {
        var claimPath = SelectSecondIndexSample;

        var sut = claimPath.ProcessWith(JsonBasedCredentialSample);
        sut.Match(
            selection =>
            {
                selection.GetValues().Single().ToObject<string>().Should().Be("Betelgeusian");
            },
            _ => Assert.Fail("ClaimPathSelection validation failed")
        );
    }

    [Fact]
    public void Can_Process_Null_Components()
    {
        var claimPath = SelectAllElementsInArraySample;

        var sut = claimPath.ProcessWith(JsonBasedCredentialSample);
        sut.Match(
            selection =>
            {
                selection.GetValues().Count().Should().Be(2);
            },
            _ => Assert.Fail("ClaimPathSelection validation failed")
        );
    }

    [Fact]
    public void Can_Process_String_Components()
    {
        var claimPath = SelectFirstNameByKeySample;

        var sut = claimPath.ProcessWith(JsonBasedCredentialSample);
        sut.Match(
            selection =>
            {
                selection.GetValues().Count().Should().Be(1);
            },
            _ => Assert.Fail("ClaimPathSelection validation failed")
        );
    }

    [Fact]
    public void Empty_Selection_Returns_In_Error()
    {
        var claimPath = NonExistentKeySample;
        
        var sut = claimPath.ProcessWith(JsonBasedCredentialSample);
        sut.Match(
            _ => Assert.Fail("Expected error, got selection"),
            errors => errors.Should().ContainSingle(e => e is SelectionIsEmptyError)
        );
    }

    [Fact]
    public void Processing_Index_Component_With_Not_An_Array_Results_In_Error()
    {
        var claimPath = IndexOnNonArraySample;
        
        var sut = claimPath.ProcessWith(JsonBasedCredentialSample);
        sut.Match(
            _ => Assert.Fail("Expected error, got selection"),
            errors => errors.Should().ContainSingle(e => e is SelectedElementIsNotAnArrayError)
        );
    }

    [Fact]
    public void Processing_Null_Component_With_Not_An_Array_Results_In_Error()
    {
        var claimPath = NullOnNonArraySample;
        
        var sut = claimPath.ProcessWith(JsonBasedCredentialSample);
        sut.Match(
            _ => Assert.Fail("Expected error, got selection"),
            errors => errors.Should().ContainSingle(e => e is SelectedElementIsNotAnArrayError)
        );
    }

    [Fact]
    public void Processing_String_Component_With_Not_An_Object_Results_In_Error()
    {
        var claimPath = StringOnNonObjectSample;
        
        var sut = claimPath.ProcessWith(JsonBasedCredentialSample);
        sut.Match(
            _ => Assert.Fail("Expected error, got selection"),
            errors => errors.Should().ContainSingle(e => e is SelectedElementIsNotAnObjectError)
        );
    }

    [Fact]
    public void Processing_Unknown_Component_Results_In_Error()
    {
        var result = ClaimPath.FromJArray([1.23]);
        result.Match(
            _ => Assert.Fail("Expected error, got selection"),
            errors => errors.Should().ContainSingle(e => e is UnknownComponentError)
        );
    }

    [Fact]
    public void ClaimPathSelection_Selects_All_Array_Elements_With_Null()
    {
        var credential = JObject.Parse(
            """
            {
              "degrees": [
                { "type": "Bachelor of Science", "university": "U1" },
                { "type": "Master of Science",   "university": "U2" }
              ]
            }
            """);

        var path = ClaimPath.FromJArray(new JArray("degrees", JValue.CreateNull(), "type")).UnwrapOrThrow();

        path.ProcessWith(credential).Match(
            selection => selection
                .GetValues()
                .Select(t => t.ToString())
                .Should()
                .BeEquivalentTo(new[] { "Bachelor of Science", "Master of Science" }),
            _ => Assert.Fail("ClaimPathSelection validation failed"));
    }

    [Fact]
    public void ClaimPathSelection_Selects_By_Integer_Index()
    {
        var credential = JObject.Parse(
            """
            { "nationalities": ["British", "Betelgeusian"] }
            """);

        var path = ClaimPath.FromJArray(new JArray("nationalities", 1)).UnwrapOrThrow();

        path.ProcessWith(credential).Match(
            selection => selection
                .GetValues()
                .Select(t => t.ToString())
                .Should()
                .BeEquivalentTo(new[] { "Betelgeusian" }),
            _ => Assert.Fail("ClaimPathSelection validation failed"));
    }

    [Fact]
    public void ClaimPathSelection_Errors_When_Component_Targets_NonArray()
    {
        var credential = JObject.Parse("""{ "name": "Arthur" }""");
        var path = ClaimPath.FromJArray(new JArray("name", JValue.CreateNull())).UnwrapOrThrow();

        path.ProcessWith(credential).Match(
            _ => Assert.Fail("Expected error, got selection"),
            errors => errors.Should().ContainSingle(e => e is SelectedElementIsNotAnArrayError));
    }
}
