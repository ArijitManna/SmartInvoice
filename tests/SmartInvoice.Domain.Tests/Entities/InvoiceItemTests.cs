using SmartInvoice.Domain.Entities;

namespace SmartInvoice.Domain.Tests.Entities;

public class InvoiceItemTests
{
    [Fact]
    public void Calculate_BasicItem_ComputesCorrectly()
    {
        var item = new InvoiceItem
        {
            Quantity = 3,
            Rate = 1000,
            TaxRate = 18
        };

        item.Calculate();

        Assert.Equal(3000m, item.Amount);
        Assert.Equal(540m, item.TaxAmount);
    }

    [Fact]
    public void Calculate_WithDiscountPercentage_AppliesDiscount()
    {
        var item = new InvoiceItem
        {
            Quantity = 2,
            Rate = 500,
            TaxRate = 12,
            DiscountPercentage = 10
        };

        item.Calculate();

        Assert.Equal(100m, item.DiscountAmount); // 10% of 1000
        Assert.Equal(900m, item.Amount);         // 1000 - 100
        Assert.Equal(108m, item.TaxAmount);      // 12% of 900
    }

    [Fact]
    public void Calculate_ZeroTax_NoTaxAmount()
    {
        var item = new InvoiceItem
        {
            Quantity = 1,
            Rate = 200,
            TaxRate = 0
        };

        item.Calculate();

        Assert.Equal(200m, item.Amount);
        Assert.Equal(0m, item.TaxAmount);
    }
}
