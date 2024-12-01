#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace IpPool.Lib;

public class IpEnvironment(IpAddr addressSpace)
{
    private readonly CidrTrie _root = new(addressSpace);
    
    public List<VirtualNetwork> VirtualNetworks { get; set; } = new List<VirtualNetwork>();

    public VirtualNetwork GetVirtualNetworkByKey(string key)
    {
        var existing = VirtualNetworks.SingleOrDefault(x => x.Key == key);
        if (existing == null)
        {
            throw new BusinessException($"virtual network not found: {key}");
        }
        return existing;
    }
    
    public VirtualNetwork AddVirtualNetwork(string key, IpAddr ip)
    {
        var vnetRoot = _root.AllocateCidr(key, ip);
        var vnet = new VirtualNetwork(key, vnetRoot);
        VirtualNetworks.Add(vnet);
        return vnet;
    }

    public VirtualNetwork AddVirtualNetwork(string key, int size)
    {
        var vnetRoot = _root.AllocateCidr(key, size);
        var vnet = new VirtualNetwork(key, vnetRoot);
        VirtualNetworks.Add(vnet);
        return vnet;
    }
    
    public class VirtualNetwork(string key, IpAddr addressSpace)
    {
        public string Key { get; } = key;
        
        public CidrTrie Root = new CidrTrie(addressSpace);

        public List<Subnet> Subnets { get; set; } = new List<Subnet>();
        
        public Subnet AddSubnet(string key, IpAddr ip)
        {
            var vnetRoot = Root.AllocateCidr(key, ip);
            var snet = new Subnet(key, vnetRoot);
            Subnets.Add(snet);
            return snet;
        }

        public Subnet AddSubnet(string key, int size)
        {
            var vnetRoot = Root.AllocateCidr(key, size);
            var snet = new Subnet(key, vnetRoot);
            Subnets.Add(snet);
            return snet;
        }
    }
    
    public class Subnet(string key, IpAddr addressSpace)
    {
        public string Key { get; } = key;
        public IpAddr AddressSpace { get; } = addressSpace;
    }
    
    public static IpEnvironment FromState(IpEnvironmentState state)
    {
        var env = new IpEnvironment(new IpAddr(state.AddressSpace));
        
        if (state.VirtualNetworks != null)
        {
            foreach (var vnetState in state.VirtualNetworks)
            {
                var vnet = env.AddVirtualNetwork(vnetState.Key, new IpAddr(vnetState.Value.AddressSpace));
                if (vnetState.Value.Subnets != null)
                {
                    foreach(var snetState in vnetState.Value.Subnets)
                    {
                        vnet.AddSubnet(snetState.Key, new IpAddr(snetState.Value.AddressSpace));
                    }
                }
            }
        }

        return env;
    }

    public IpEnvironmentState ToState()
    {
        var state = new IpEnvironmentState();
        state.AddressSpace = _root.RootIp.ToString();
        foreach(var virtualNetwork in VirtualNetworks)
        {
            var vnetState = new IpEnvironmentState.VirtualNetwork();
            vnetState.AddressSpace = virtualNetwork.Root.RootIp.ToString();
            foreach(var subnet in virtualNetwork.Subnets)
            {
                var snetState = new IpEnvironmentState.Subnet();
                snetState.AddressSpace = subnet.AddressSpace.ToString();
                vnetState.Subnets.Add(subnet.Key, snetState);
            }
            state.VirtualNetworks.Add(virtualNetwork.Key, vnetState);
        }
        return state;
    }
}