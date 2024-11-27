using FluentAssertions;
using IpPool.Lib;

namespace IpPool.Tests;

[TestClass]
public class ParsingTests
{
    [TestMethod]
    public void CanParseIpFromInt()
    {
        var ip = new IpAddr("127.0.0.1");
        ip.PrefixSize.Should().BeNull();
        ip.ToString().Should().Be("127.0.0.1");
        
        ip = new IpAddr("127.0.0.1/0");
        ip.PrefixSize.Should().Be(0);
        ip.ToString().Should().Be("0.0.0.0/0");
        
        ip = new IpAddr("127.0.1.1/24");
        ip.PrefixSize.Should().Be(24);
        ip.ToString().Should().Be("127.0.1.0/24");
        
        ip = new IpAddr("127.1.1.1/16");
        ip.PrefixSize.Should().Be(16);
        ip.ToString().Should().Be("127.1.0.0/16");
    }
}