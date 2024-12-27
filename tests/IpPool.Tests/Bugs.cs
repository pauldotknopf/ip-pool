using IpPool.Lib;

namespace IpPool.Tests;

[TestClass]
public class Bugs
{
    public TestContext TestContext { get; set; }
    
    [TestMethod]
    public void CantAddDuplicateKeys()
    {
        var ipEnv = new IpEnvironment(new IpAddr("10.0.0.0/28"));
        var vnet = ipEnv.AddVirtualNetwork("vnet", new IpAddr("10.0.0.0/28"));
        
        var debug1 = vnet.DebugOutput();
        TestContext.WriteLine(debug1);
        
        var snet1 = vnet.AddSubnet("snet1", 2);
        var debug2 = vnet.DebugOutput();
        TestContext.WriteLine(debug2);
        
        var snet2 = vnet.AddSubnet("snet2", 1);
        var debug3 = vnet.DebugOutput();
        TestContext.WriteLine(debug3);
        
        var snet3 = vnet.AddSubnet("snet3", 2);
        var debug4 = vnet.DebugOutput();
        TestContext.WriteLine(debug4);

        var newEnv = new IpEnvironment(ipEnv.AddressSpace);
        vnet = newEnv.AddVirtualNetwork("vnet", vnet.AddressSpace);
        vnet.AddSubnet("snet1", snet1.AddressSpace);
        vnet.AddSubnet("snet2", snet2.AddressSpace);
        vnet.AddSubnet("snet3", snet3.AddressSpace);
    }
}