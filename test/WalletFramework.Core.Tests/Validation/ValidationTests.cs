using FluentAssertions;
using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using Xunit;

namespace WalletFramework.Core.Tests.Validation;

public class ValidationTests
{
    [Fact]
    public void AggregationWorks()
    {
        Validator<int> greaterThanZero = i => i > 0
            ? ValidationFun.Valid(i)
            : new SampleError();
        
        Validator<int> isEven = i => i % 2 == 0
            ? ValidationFun.Valid(i)
            : new SampleError();

        var sut = new List<Validator<int>> { greaterThanZero, isEven }.AggregateValidators();

        var one = sut(1);
        var two = sut(2);

        one.IsSuccess.Should().BeFalse();
        two.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void FallbackWorks()
    {
        var one = new SampleError().ToInvalid<Validation<int>>();

        var sut = one.Fallback(2);

        sut.UnwrapOrThrow().Should().Be(2);
    }

    [Fact]
    public void FirstValidWorks()
    {
        const string one = "1";
        const string nan = "NaN";
        Validator<string, int> valid = str =>
        {
            try
            {
                return ValidationFun.Valid(int.Parse(str));
            }
            catch (Exception )
            {
                return new SampleError();
            }
        };
        Validator<string, int> invalid = _ => new SampleError();

        var sut = new List<Validator<string, int>> { invalid, valid }.FirstValid();

        var oneValid = sut(one);
        var nanInvalid = sut(nan);

        oneValid.UnwrapOrThrow().Should().Be(1);
        nanInvalid.Match(
            i => Assert.Fail("Validation must fail"),
            errors =>
            {
                errors.Should().AllBeOfType<NoItemsSucceededValidationError<int>>();
                errors.Should().ContainSingle();
            } 
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MatchWorks(bool valid)
    {
        Validation<int> validation = valid
            ? 1
            : new SampleError();

        validation.Match(
            _ => valid.Should().BeTrue(),
            errors =>
            {
                valid.Should().BeFalse();
                errors.Should().AllBeOfType<SampleError>();
                errors.Should().ContainSingle();
            });
    }

    [Fact]
    public void OnSuccessWorks()
    {
        var one = ValidationFun.Valid("1");

        var sut = one.OnSuccess(int.Parse);

        sut.UnwrapOrThrow().Should().Be(1);
    }
    
    [Fact]
    public async Task OnSuccessAsyncWorks()
    {
        var one = ValidationFun.Valid("1").AsTask();

        var sut = await one.OnSuccess(int.Parse);

        sut.UnwrapOrThrow().Should().Be(1);
    }

    [Fact]
    public void SelectManyWorks()
    {
        var one = ValidationFun.Valid("1");

        var sut = one.SelectMany(
            _ => ValidationFun.Valid(1),
            (e1, e2) => int.Parse(e1) + e2
        );

        sut.UnwrapOrThrow().Should().Be(2);
    }

    [Fact]
    public void SelectWorks()
    {
        var one = ValidationFun.Valid("1");

        var sut = one.Select(int.Parse);

        sut.Match(
            i => i.Should().Be(1),
            _ => Assert.Fail("Validation must not fail"));
    }

    [Fact]
    public void TraverseAllWorks()
    {
        var validStrs = new List<string>
        {
            "1",
            "2",
            "3"
        };
        
        var invalidStrs = new List<string>
        {
            "1",
            "2",
            "Three"
        };

        var sutValid = validStrs.TraverseAll(s =>
        {
            try
            {
                return ValidationFun.Valid(int.Parse(s));
            }
            catch (Exception)
            {
                return new SampleError();
            }
        });
        
        var sutInvalid = invalidStrs.TraverseAll(str =>
        {
            try
            {
                return ValidationFun.Valid(int.Parse(str));
            }
            catch (Exception)
            {
                return new SampleError();
            }
        });

        sutValid.IsSuccess.Should().BeTrue();
        sutInvalid.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void TraverseAnyWorks()
    {
        var strs = new List<string>
        {
            "One",
            "2",
            "Three"
        };

        var sut = strs.TraverseAny(str =>
        {
            try
            {
                return ValidationFun.Valid(int.Parse(str));
            }
            catch (Exception)
            {
                return new SampleError();
            }
        });

        sut.Match(
            ints =>
            {
                var list = ints.ToList();
                
                list.Should().ContainSingle();
                list.First().Should().Be(2);
            },
            errors =>
            {
                Assert.Fail("Validation must not fail");
                errors.Should().ContainSingle();
                errors.Should().AllBeOfType<SampleError>();
            }
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UnwrapOrThrowWorks(bool valid)
    {
        Validation<int> validation = valid
            ? 1
            : new SampleError();

        try
        {
            validation.UnwrapOrThrow();
            valid.Should().BeTrue();
        }
        catch (Exception)
        {
            valid.Should().BeFalse();
        }
    }

    private record SampleError() : Error("This is sample error for testing");
}
