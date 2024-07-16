using FluentAssertions;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;
using WalletFramework.Oid4Vc.Tests.Oid4Vci.Localization.Samples;
using static System.Collections.Immutable.ImmutableDictionary;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.Localization;

public class LocaleTests
{
    [Theory]
    [InlineData("en-US", "en-US")]
    [InlineData("en", "en-US")]
    [InlineData("de-DE", "de-DE")]
    [InlineData("de", "de-DE")]
    public void Can_Create_From_Valid_Input(string input, string expected) => Locale.ValidLocale(input).Match(
        sut =>
        {
            sut.ToString().Should().Be(expected);
        },
        _ => Assert.Fail("Locale is invalid"));

    [Theory]
    [InlineData("Peter")]
    [InlineData("")]
    [InlineData("english")]
    public void Invalid_Input_Is_Not_Allowed(string invalidInput) => Locale.ValidLocale(invalidInput).Match(
        _ => Assert.Fail("Invalid input must not be able to create locale"),
        _ => { });

    [Fact]
    public void Can_Find_Matching_Locale_In_A_Dictionary()
    {
        var english = LocaleSample.English;
        var dictionary = CreateRange(new[]
        {
            KeyValuePair.Create(LocaleSample.German, 1),
            KeyValuePair.Create(english, 0)
        });
        
        var sut = dictionary.FindOrDefault(english);
        
        sut.Should().Be(0);
    }

    [Fact]
    public void Get_English_As_Fallback_When_No_Matching_Locale_Is_Found_In_A_Dictionary()
    {
        var dictionary = CreateRange(new[]
            {
                KeyValuePair.Create(LocaleSample.German, 1),
                KeyValuePair.Create(LocaleSample.English, 0)
            }
        );
        
        var sut = dictionary.FindOrDefault(LocaleSample.Japanese);
        
        sut.Should().Be(0);
    }
        
    [Fact]
    public void Get_First_Entry_As_Fallback_When_No_Matching_Locale_And_No_English_Is_Found_In_A_Dictionary()
    {
        var dictionary = CreateRange(new[]
            {
                KeyValuePair.Create(LocaleSample.German, 1),
                KeyValuePair.Create(LocaleSample.Japanese, 2)
            }
        );
        
        var sut = dictionary.FindOrDefault(LocaleSample.Korean);
        
        sut.Should().BeGreaterOrEqualTo(1);
    }
}
