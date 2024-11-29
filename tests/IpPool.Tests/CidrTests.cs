using System.Text;
using FluentAssertions;
using IpPool.Lib;

namespace IpPool.Tests;

[TestClass]
public class CidrTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void CantAddDuplicateKeys()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/24"));
        cidr.AllocateCidr(1, "Test1");
        Assert.ThrowsException<BusinessException>(() => cidr.AllocateCidr(1, "test1"));
        cidr.AllocateCidr(1, "test2");
    }
    
    [TestMethod]
    public void CantAllocateToLargeOfPool()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/24"));
        Assert.ThrowsException<BusinessException>(() => cidr.AllocateCidr(9, "test1"));
    }
    
    [TestMethod]
    public void CanAllocateExactSizeFromPool()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/24"));
        var ip = cidr.AllocateCidr(8, "test1");
        ip.ToString().Should().Be("127.0.0.0/24");
    }
    
    [TestMethod]
    public void CanAllocateSmallerSizeFromPool()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/24"));
        var ip = cidr.AllocateCidr(7, "test1");
        ip.ToString().Should().Be("127.0.0.0/25");
    }
    
    [TestMethod]
    public void CanAllocateTwoHalfsFromThePool_30()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/30"));
        TestContext.WriteLine(cidr.DebugOutput());
        var ip = cidr.AllocateCidr(1, "test1");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.0/31");
        ip = cidr.AllocateCidr(1, "test2");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.2/31");
    }
    
    [TestMethod]
    public void CanAllocateTwoHalfsFromThePool_29()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/29"));
        var ip = cidr.AllocateCidr(2, "test1");
        ip.ToString().Should().Be("127.0.0.0/30");
        ip = cidr.AllocateCidr(2, "test2");
        ip.ToString().Should().Be("127.0.0.4/30");
    }
    
    [TestMethod]
    public void CanAllocateTwoHalfsFromThePool_8()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        var ip = cidr.AllocateCidr(23, "test1");
        ip.ToString().Should().Be("127.0.0.0/9");
        ip = cidr.AllocateCidr(23, "test2");
        ip.ToString().Should().Be("127.128.0.0/9");
    }
    
    [TestMethod]
    public void CanAllocate4QuartersFromThePool_29()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/29"));
        TestContext.WriteLine(cidr.DebugOutput());
        var ip = cidr.AllocateCidr(1, "test1");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.0/31");
        ip = cidr.AllocateCidr(1, "test2");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.2/31");
        ip = cidr.AllocateCidr(1, "test3");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.4/31");
        ip = cidr.AllocateCidr(1, "test4");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.6/31");
    }
    
    [TestMethod]
    public void CanAllocate4QuartersFromThePool_28()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/28"));
        TestContext.WriteLine(cidr.DebugOutput());
        var ip = cidr.AllocateCidr(2, "test1");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.0/30");
        ip = cidr.AllocateCidr(2, "test2");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.4/30");
        ip = cidr.AllocateCidr(2, "test3");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.8/30");
        ip = cidr.AllocateCidr(2, "test4");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.12/30");
    }
    
    [TestMethod]
    public void CanAllocate8EightsFromThePool_28()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/28"));
        TestContext.WriteLine(cidr.DebugOutput());
        var ip = cidr.AllocateCidr(1, "test1");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.0/31");
        ip = cidr.AllocateCidr(1, "test2");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.2/31");
        ip = cidr.AllocateCidr(1, "test3");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.4/31");
        ip = cidr.AllocateCidr(1, "test4");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.6/31");
        ip = cidr.AllocateCidr(1, "test5");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.8/31");
        ip = cidr.AllocateCidr(1, "test6");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.10/31");
        ip = cidr.AllocateCidr(1, "test7");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.12/31");
        ip = cidr.AllocateCidr(1, "test8");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.14/31");
    }
    
    [TestMethod]
    public void CanAllocateThreeSubnets()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        var ip = cidr.AllocateCidr(23, "test1");
        //TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.0.0.0/9");
        ip = cidr.AllocateCidr(22, "test2");
        //TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.128.0.0/10");
        ip = cidr.AllocateCidr(22, "test3");
        TestContext.WriteLine(cidr.DebugOutput());
        ip.ToString().Should().Be("127.192.0.0/10");
    }

    [TestMethod]
    public void CantAllocateIpWithSmallerPrefix()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        var message = Assert.ThrowsException<BusinessException>(() => cidr.AllocateCidr(new IpAddr("127.0.0.0/7"), "test"));
        message.Message.Should().Be("prefix size is too small: 126.0.0.0/7");
    }
    
    [TestMethod]
    public void CantAllocateIpWithDifferntPrefix()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        var message = Assert.ThrowsException<BusinessException>(() => cidr.AllocateCidr(new IpAddr("255.0.0.0/8"), "test"));
        message.Message.Should().Be("IP prefix does not match the root IP prefix: 255.0.0.0/8");
    }
    
    [TestMethod]
    public void CantAllocateRootIp()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        var message = Assert.ThrowsException<BusinessException>(() => cidr.AllocateCidr(new IpAddr("255.0.0.0/8"), "test"));
        message.Message.Should().Be("IP prefix does not match the root IP prefix: 255.0.0.0/8");
    }

    [TestMethod]
    public void CanAllocateRootIp()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        var result = cidr.AllocateCidr(new IpAddr("127.0.0.0/8"), "test1");
        result.ToString().Should().Be("127.0.0.0/8");
    }
    
    [TestMethod]
    public void CanAllocateNestedIp()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        var result = cidr.AllocateCidr(new IpAddr("127.35.0.0/16"), "test1");
        result.ToString().Should().Be("127.35.0.0/16");
        var state = cidr.GetState();
        state.Reserved.Should().ContainKey("test1").WhoseValue.Should().Be("127.35.0.0/16");
    }
    
    [TestMethod]
    public void CantAllocateAlreadyReservedIp()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        cidr.AllocateCidr(new IpAddr("127.35.0.0/16"), "test1");
        var message = Assert.ThrowsException<BusinessException>(() => cidr.AllocateCidr(new IpAddr("127.35.0.0/16"), "test2"));
        message.Message.Should().Be("the requested reservation 127.35.0.0/16 conflicts with 127.35.0.0/16");
    }
    
    [TestMethod]
    public void CantAllocateAlreadyReservedIp2()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        cidr.AllocateCidr(new IpAddr("127.35.0.0/16"), "test1");
        var message = Assert.ThrowsException<BusinessException>(() => cidr.AllocateCidr(new IpAddr("127.32.0.0/12"), "test2"));
        message.Message.Should().Be("the requested reservation 127.32.0.0/12 conflicts with 127.35.0.0/16");
    }

    [TestMethod]
    public void CanGetState()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        var ip1 = cidr.AllocateCidr(8, "test1");
        var ip2 = cidr.AllocateCidr(8, "test2");

        var state = cidr.GetState();
        state.Reserved.Should().HaveCount(2);
        state.Reserved.Should().ContainKey("test1").WhoseValue.Should().Be(ip1.ToString());
        state.Reserved.Should().ContainKey("test2").WhoseValue.Should().Be(ip2.ToString());
    }

    [TestMethod]
    public void CanLoadFromState()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/8"));
        var ip1 = cidr.AllocateCidr(8, "test1");
        var ip2 = cidr.AllocateCidr(8, "test2");

        var state = new CidrState();
        state.Pool = "127.0.0.0/8";
        state.Reserved = new Dictionary<string, string>();
        state.Reserved["test1"] = ip1.ToString();
        state.Reserved["test2"] = ip2.ToString();

        var loaded = CidrTrie.FromState(state);
        state = loaded.GetState();

        state.Pool.Should().Be("127.0.0.0/8");
        state.Reserved.Should().ContainKey("test1").WhoseValue.Should().Be(ip1.ToString());
        state.Reserved.Should().ContainKey("test2").WhoseValue.Should().Be(ip2.ToString());
    }

    [TestMethod]
    public void CanAllocateEverything()
    {
        var cidr = new CidrTrie(new IpAddr("127.0.0.0/24"));
        for (int x = 0; x < 16; x++)
        {
            var ip = cidr.AllocateCidr(4, $"test{x}");
        }

        var message = Assert.ThrowsException<BusinessException>(() => cidr.AllocateCidr(4, "sdf"));
        message.Message.Should().Be("couldn't find a suitable CIDR block");
    }
}