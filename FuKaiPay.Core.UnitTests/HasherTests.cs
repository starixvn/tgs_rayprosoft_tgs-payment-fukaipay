using Microsoft.Extensions.Logging.Abstractions;
using Tgs.Payment.FuKaiPay;

namespace FuKaiPay.Core.UnitTests;

public class HasherTests
{
    [Fact]
    public void 餘額查詢()
    {
        var dict = new Dictionary<string, string?>
        {
            ["MerchantAccount"] = "merchantId",
        };

        var sut = new FuKaiPayHasher(NullLogger<FuKaiPayHasher>.Instance);

        var actual = sut.ComputeHash("merchantKey", dict);

        var expected = "2dgyoceDK/2C8SoUp15rU5PK91B0B3BMwWGHmwTMUUw=";

        Assert.Equal(expected, actual);
    }
}