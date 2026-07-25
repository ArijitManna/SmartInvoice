using SmartInvoice.Domain.ValueObjects;

namespace SmartInvoice.Domain.Tests.ValueObjects;

public class GstInfoTests
{
    [Fact]
    public void IsValid_WithValidGstin_ReturnsTrue()
    {
        var gst = new GstInfo { Gstin = "29ABCDE1234F1Z5" };

        Assert.True(gst.IsValid());
    }

    [Fact]
    public void IsValid_WithInvalidLength_ReturnsFalse()
    {
        var gst = new GstInfo { Gstin = "29ABC" };

        Assert.False(gst.IsValid());
    }

    [Fact]
    public void IsValid_WithNull_ReturnsTrue()
    {
        var gst = new GstInfo { Gstin = null };

        Assert.True(gst.IsValid());
    }

    [Fact]
    public void GetStateCodeFromGstin_ReturnsFirst2Chars()
    {
        var gst = new GstInfo { Gstin = "29ABCDE1234F1Z5" };

        Assert.Equal("29", gst.GetStateCodeFromGstin());
    }

    [Fact]
    public void GetStateCodeFromGstin_WithNull_ReturnsNull()
    {
        var gst = new GstInfo { Gstin = null };

        Assert.Null(gst.GetStateCodeFromGstin());
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new GstInfo { Gstin = "29ABCDE1234F1Z5", Pan = "ABCDE1234F" };
        var b = new GstInfo { Gstin = "29ABCDE1234F1Z5", Pan = "ABCDE1234F" };

        Assert.Equal(a, b);
    }
}
