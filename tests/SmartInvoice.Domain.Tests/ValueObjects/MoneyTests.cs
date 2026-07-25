using SmartInvoice.Domain.ValueObjects;

namespace SmartInvoice.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidAmount_CreatesMoney()
    {
        var money = new Money(100.50m, "INR");

        Assert.Equal(100.50m, money.Amount);
        Assert.Equal("INR", money.Currency);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Money(-10, "INR"));
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        var a = new Money(100, "INR");
        var b = new Money(50, "INR");

        var result = a.Add(b);

        Assert.Equal(150m, result.Amount);
        Assert.Equal("INR", result.Currency);
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsException()
    {
        var inr = new Money(100, "INR");
        var usd = new Money(50, "USD");

        Assert.Throws<InvalidOperationException>(() => inr.Add(usd));
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        var a = new Money(100, "INR");
        var b = new Money(30, "INR");

        var result = a.Subtract(b);

        Assert.Equal(70m, result.Amount);
    }

    [Fact]
    public void Multiply_ReturnsProduct()
    {
        var money = new Money(100, "INR");

        var result = money.Multiply(0.18m);

        Assert.Equal(18m, result.Amount);
    }

    [Fact]
    public void Round_RoundsToTwoDecimals()
    {
        var money = new Money(100.555m, "INR");

        var result = money.Round();

        Assert.Equal(100.56m, result.Amount);
    }

    [Fact]
    public void Zero_CreatesZeroMoney()
    {
        var money = Money.Zero();

        Assert.Equal(0m, money.Amount);
        Assert.Equal("INR", money.Currency);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new Money(100, "INR");
        var b = new Money(100, "INR");

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = new Money(100, "INR");
        var b = new Money(200, "INR");

        Assert.NotEqual(a, b);
    }
}
