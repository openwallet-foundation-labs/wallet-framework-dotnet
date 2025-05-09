using FluentAssertions;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.ClaimPaths.Errors;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.ClaimPaths;
using static WalletFramework.Oid4Vc.Tests.Oid4Vp.ClaimPaths.Samples.ClaimPathSamples;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.ClaimPaths;

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
}
