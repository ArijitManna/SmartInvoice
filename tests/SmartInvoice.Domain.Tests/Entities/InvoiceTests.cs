using SmartInvoice.Domain.Entities;
using SmartInvoice.Domain.Enums;

namespace SmartInvoice.Domain.Tests.Entities;

public class InvoiceTests
{
    private static Invoice CreateInvoiceWithItems()
    {
        var invoice = new Invoice();
        var item1 = new InvoiceItem
        {
            Quantity = 2,
            Rate = 1000,
            TaxRate = 18,
            Unit = "Nos"
        };
        item1.Calculate();

        var item2 = new InvoiceItem
        {
            Quantity = 1,
            Rate = 500,
            TaxRate = 18,
            Unit = "Nos"
        };
        item2.Calculate();

        invoice.Items = [item1, item2];
        return invoice;
    }

    [Fact]
    public void Recalculate_IntraState_SplitsCgstSgst()
    {
        var invoice = CreateInvoiceWithItems();

        invoice.Recalculate("KARNATAKA", "KARNATAKA");

        Assert.Equal(GstType.IntraState, invoice.GstType);
        Assert.Equal(2500m, invoice.SubTotal);
        Assert.Equal(450m, invoice.TaxAmount);
        Assert.Equal(225m, invoice.CgstAmount);
        Assert.Equal(225m, invoice.SgstAmount);
        Assert.Equal(0m, invoice.IgstAmount);
        Assert.Equal(2950m, invoice.TotalAmount);
    }

    [Fact]
    public void Recalculate_InterState_UsesIgst()
    {
        var invoice = CreateInvoiceWithItems();

        invoice.Recalculate("KARNATAKA", "MAHARASHTRA");

        Assert.Equal(GstType.InterState, invoice.GstType);
        Assert.Equal(0m, invoice.CgstAmount);
        Assert.Equal(0m, invoice.SgstAmount);
        Assert.Equal(450m, invoice.IgstAmount);
        Assert.Equal(2950m, invoice.TotalAmount);
    }

    [Fact]
    public void Recalculate_WithDiscount_AppliesBeforeTax()
    {
        var invoice = CreateInvoiceWithItems();
        invoice.DiscountPercentage = 10;

        invoice.Recalculate("KARNATAKA", "KARNATAKA");

        Assert.Equal(250m, invoice.DiscountAmount);
        // Total = (2500 - 250) + 450 = 2700
        Assert.Equal(2700m, invoice.TotalAmount);
    }

    [Fact]
    public void RecordPayment_PartialPayment_UpdatesStatusToPartiallyPaid()
    {
        var invoice = CreateInvoiceWithItems();
        invoice.Recalculate("KARNATAKA", "KARNATAKA");

        invoice.RecordPayment(1000);

        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
        Assert.Equal(1000m, invoice.AmountPaid);
        Assert.Equal(1950m, invoice.BalanceDue);
    }

    [Fact]
    public void RecordPayment_FullPayment_UpdatesStatusToPaid()
    {
        var invoice = CreateInvoiceWithItems();
        invoice.Recalculate("KARNATAKA", "KARNATAKA");

        invoice.RecordPayment(2950);

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(0m, invoice.BalanceDue);
    }

    [Fact]
    public void RecordPayment_ExceedsBalance_ThrowsException()
    {
        var invoice = CreateInvoiceWithItems();
        invoice.Recalculate("KARNATAKA", "KARNATAKA");

        Assert.Throws<InvalidOperationException>(() => invoice.RecordPayment(5000));
    }

    [Fact]
    public void RecordPayment_ZeroAmount_ThrowsException()
    {
        var invoice = CreateInvoiceWithItems();
        invoice.Recalculate("KARNATAKA", "KARNATAKA");

        Assert.Throws<ArgumentException>(() => invoice.RecordPayment(0));
    }

    [Fact]
    public void Cancel_DraftInvoice_Succeeds()
    {
        var invoice = new Invoice { Status = InvoiceStatus.Draft };

        invoice.Cancel();

        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
    }

    [Fact]
    public void Cancel_PaidInvoice_ThrowsException()
    {
        var invoice = new Invoice { Status = InvoiceStatus.Paid };

        Assert.Throws<InvalidOperationException>(() => invoice.Cancel());
    }
}
