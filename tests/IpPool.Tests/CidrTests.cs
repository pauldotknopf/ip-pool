using System.Text;
using FluentAssertions;
using IpPool.Lib;

namespace IpPool.Tests;

[TestClass]
public class CidrTests
{
    public TestContext TestContext { get; set; }
    
    [TestMethod]
    public void CantAllocateToLargeOfPool()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/24"));
        Assert.ThrowsException<BusinessException>(() => cidr.AllocateCidr(9));
    }
    
    [TestMethod]
    public void CanAllocateExactSizeFromPool()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/24"));
        var ip = cidr.AllocateCidr(8);
        ip.ToString().Should().Be("127.0.0.0/24");
    }
    
    [TestMethod]
    public void CanAllocateSmallerSizeFromPool()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/24"));
        var ip = cidr.AllocateCidr(7);
        ip.ToString().Should().Be("127.0.0.0/25");
    }
    
    [TestMethod]
    public void CanAllocateTwoHalfsFromThePool_30()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/30"));
        TestContext.WriteLine(cidr.DebugOutput());
        var ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.0/31");
        ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.2/31");
    }
    
    [TestMethod]
    public void CanAllocateTwoHalfsFromThePool_29()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/29"));
        var ip = cidr.AllocateCidr(2);
        ip.ToString().Should().Be("127.0.0.0/30");
        ip = cidr.AllocateCidr(2);
        ip.ToString().Should().Be("127.0.0.4/30");
    }
    
    [TestMethod]
    public void CanAllocateTwoHalfsFromThePool_8()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        var ip = cidr.AllocateCidr(23);
        ip.ToString().Should().Be("127.0.0.0/9");
        ip = cidr.AllocateCidr(23);
        ip.ToString().Should().Be("127.128.0.0/9");
    }
    
    [TestMethod]
    public void CanAllocate4QuartersFromThePool_29()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/29"));
        TestContext.WriteLine(cidr.DebugOutput());
        var ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.0/31");
        ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.2/31");
        ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.4/31");
        ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.6/31");
    }
    
    [TestMethod]
    public void CanAllocate4QuartersFromThePool_28()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/28"));
        TestContext.WriteLine(cidr.DebugOutput());
        var ip = cidr.AllocateCidr(2);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.0/30");
        ip = cidr.AllocateCidr(2);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.4/30");
        ip = cidr.AllocateCidr(2);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.8/30");
        ip = cidr.AllocateCidr(2);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.12/30");
    }
    
    [TestMethod]
    public void CanAllocate8EightsFromThePool_28()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/28"));
        TestContext.WriteLine(cidr.DebugOutput());
        var ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.0/31");
        ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.2/31");
        ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.4/31");
        ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.6/31");
        ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.8/31");
        ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.10/31");
        ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.12/31");
        ip = cidr.AllocateCidr(1);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.14/31");
    }
    
    [TestMethod]
    public void CanAllocateThreeSubnets()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        var ip = cidr.AllocateCidr(23);
        //TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.0/9");
        ip = cidr.AllocateCidr(22);
        //TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.128.0.0/10");
        ip = cidr.AllocateCidr(22);
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.192.0.0/10");
    }
}