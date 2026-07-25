using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Domain.Tests.Entities;

public class CompanyTests
{
    [Fact]
    public void GenerateInvoiceNumber_ReturnsFormattedNumber()
    {
        var company = new Company
        {
            InvoicePrefix = "INV",
            NextInvoiceNumber = 1
        };

        string number = company.GenerateInvoiceNumber();

        Assert.StartsWith("INV-", number);
        Assert.Contains("-0001", number);
    }

    [Fact]
    public void GenerateInvoiceNumber_IncrementsCounter()
    {
        var company = new Company { NextInvoiceNumber = 5 };

        company.GenerateInvoiceNumber();

        Assert.Equal(6, company.NextInvoiceNumber);
    }

    [Fact]
    public void GenerateInvoiceNumber_SequentialNumbers_AreUnique()
    {
        var company = new Company { InvoicePrefix = "INV", NextInvoiceNumber = 1 };

        string first = company.GenerateInvoiceNumber();
        string second = company.GenerateInvoiceNumber();

        Assert.NotEqual(first, second);
    }
}
