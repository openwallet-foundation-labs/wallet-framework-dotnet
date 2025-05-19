using FluentAssertions;
using WalletFramework.Core.Functional;
using Xunit;

namespace WalletFramework.Core.Tests.Validation;

public class ApplyTests
{
    private record Sample(int X1, int X2, int X3, int X4, int X5, int X6, int X7)
    {
        public int Sum() => X1 + X2 + X3 + X4 + X5 + X6 + X7;
    }
    
    private static Sample CreateSample(int x1, int x2, int x3, int x4, int x5, int x6, int x7) =>
        new(x1, x2, x3, x4, x5, x6, x7);
    
    [Fact]
    public void ApplyWorks()
    {
        const int expected = 1 + 2 + 3 + 4 + 5 + 6 + 7;
        var func = ValidationFun.Valid(CreateSample);

        var sut = func
            .Apply(1)
            .Apply(2)
            .Apply(3)
            .Apply(4)
            .Apply(5)
            .Apply(6)
            .Apply(7);

        sut.UnwrapOrThrow().Sum().Should().Be(expected);
    }
}
