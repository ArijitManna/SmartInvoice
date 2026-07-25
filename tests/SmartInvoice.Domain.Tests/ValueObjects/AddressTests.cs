using SmartInvoice.Domain.ValueObjects;

namespace SmartInvoice.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void GetStateCode_ReturnsUpperCaseTrimmed()
    {
        var address = new Address { State = "  karnataka  " };

        Assert.Equal("KARNATAKA", address.GetStateCode());
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new Address { Street = "MG Road", City = "Bangalore", State = "Karnataka", PostalCode = "560001" };
        var b = new Address { Street = "MG Road", City = "Bangalore", State = "Karnataka", PostalCode = "560001" };

        Assert.Equal(a, b);
    }

    [Fact]
    public void DefaultCountry_IsIndia()
    {
        var address = new Address();

        Assert.Equal("India", address.Country);
    }
}
