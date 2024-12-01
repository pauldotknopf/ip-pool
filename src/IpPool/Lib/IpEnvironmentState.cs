namespace IpPool.Lib;

public class IpEnvironmentState
{
    public string AddressSpace { get; set; }

    public Dictionary<string, VirtualNetwork> VirtualNetworks { get; set; } = new Dictionary<string, VirtualNetwork>();
    
    public class VirtualNetwork
    {
        public string AddressSpace { get; set; }

        public Dictionary<string, Subnet> Subnets { get; set; } = new Dictionary<string, Subnet>();
    }

    public class Subnet
    {
        public string AddressSpace { get; set; }
    }
}