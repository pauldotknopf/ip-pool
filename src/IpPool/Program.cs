using System.CommandLine;
using System.Text;
using System.Text.Json;
using IpPool.Lib;

public class Program
{
    private static void SaveState(string stateFile, IpEnvironmentState state)
    {
        if (File.Exists(stateFile))
        {
            File.Delete(stateFile);
        }
        File.WriteAllText(stateFile, JsonSerializer.Serialize(state, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static void PrintException(Exception ex)
    {
        if (ex is BusinessException businessException)
        {
            Console.WriteLine(businessException.Message);
            Environment.Exit(1);
        }
        else
        {
            Console.WriteLine($"unhandled exception: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
    
    private static IpEnvironment LoadState(string stateFile)
    {
        if (!File.Exists(stateFile))
        {
            throw new BusinessException($"state file {stateFile} doesn't exist");
        }

        var state = JsonSerializer.Deserialize<IpEnvironmentState>(File.ReadAllText(stateFile),
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        return IpEnvironment.FromState(state);
    }
    
    public static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand("manage a pool of CIDR blocks");

        var stateFileLocation = new Option<string>
            (name: "--state-file",
            description: "the location to the state file")
        {
            IsRequired = true
        };
        
        var key = new Option<string>
            (name: "--key",
            description: "the key to use for the reservation")
        {
            IsRequired = true
        };

        var ip = new Option<string>
            (name: "--ip",
            description: "the ip (in CIDR notation) to reserve");
        
        var size = new Option<int?>
            (name: "--size",
            description: "the size of the CIDR to reserve");

        {
            var pool = new Option<string>
                (name: "--pool",
                description: "the CIDR pool to create")
            {
                IsRequired = true
            };
            
            var createPoolCommand = new Command("create-pool");
            createPoolCommand.AddOption(stateFileLocation);
            createPoolCommand.AddOption(pool);
            createPoolCommand.SetHandler((stateFileValue, poolValue) =>
            {
                try
                {
                    var env = new IpEnvironment(new IpAddr(poolValue));
                    SaveState(stateFileValue, env.ToState());
                }
                catch (Exception ex)
                {
                    PrintException(ex);
                }
            }, stateFileLocation, pool);
            rootCommand.Add(createPoolCommand);
        }

        {
            var reserveVnetCommand = new Command("reserve-vnet");
            reserveVnetCommand.AddOption(stateFileLocation);
            reserveVnetCommand.AddOption(ip);
            reserveVnetCommand.AddOption(size);
            reserveVnetCommand.AddOption(key);
            reserveVnetCommand.SetHandler((stateFileValue, keyValue, ipValue, sizeValue) =>
            {
                try
                {
                    var env = LoadState(stateFileValue);
                    IpEnvironment.VirtualNetwork vnet;
                    if (sizeValue.HasValue)
                    {
                        vnet = env.AddVirtualNetwork(keyValue, sizeValue.Value);
                    }
                    else
                    {
                        vnet = env.AddVirtualNetwork(keyValue, new IpAddr(ipValue));
                    }
                    Console.WriteLine("reserved: " + vnet.Root.RootIp);
                    SaveState(stateFileValue, env.ToState());
                }
                catch (Exception ex)
                {
                    PrintException(ex);
                }
            }, stateFileLocation, key, ip, size);
            rootCommand.Add(reserveVnetCommand);
        }
        
        {
            var vnetKey = new Option<string>
            (name: "--vnet-key")
            {
                IsRequired = true
            };
            
            var reserveSubnetCommand = new Command("reserve-subnet");
            reserveSubnetCommand.AddOption(stateFileLocation);
            reserveSubnetCommand.AddOption(vnetKey);
            reserveSubnetCommand.AddOption(size);
            reserveSubnetCommand.AddOption(key);
            reserveSubnetCommand.AddOption(ip);
            reserveSubnetCommand.SetHandler((stateFileValue, vnetKeyValue, keyValue, ipValue, sizeValue) =>
            {
                try
                {
                    var env = LoadState(stateFileValue);
                    var vnet = env.GetVirtualNetworkByKey(vnetKeyValue);
                    var snet = sizeValue.HasValue 
                        ? vnet.AddSubnet(keyValue, sizeValue.Value) 
                        : vnet.AddSubnet(keyValue, new IpAddr(ipValue));
                    Console.WriteLine("reservedd: " + snet.AddressSpace);
                    SaveState(stateFileValue, env.ToState());
                }
                catch (Exception ex)
                {
                    PrintException(ex);
                }
            }, stateFileLocation, vnetKey, key, ip, size);
            rootCommand.Add(reserveSubnetCommand);
        }
        
        {
            var variableName = new Option<string>
                (name: "--variable-name",
                description: "the tf variable name to put the ip addresses")
            {
                IsRequired = true
            };
            
            var generateTfCommand = new Command("generate-tf");
            generateTfCommand.AddOption(stateFileLocation);
            generateTfCommand.AddOption(variableName);
            generateTfCommand.SetHandler((stateFileValue, variableName) =>
            {
                try
                {
                    var env = LoadState(stateFileValue).ToState();
                    
                    var tfFile = Path.ChangeExtension(stateFileValue, ".tf");
                    if (Path.Exists(tfFile))
                    {
                        File.Delete(tfFile);
                    }

                    var builder = new StringBuilder();
                    builder.AppendLine("locals {");
                    builder.AppendLine($"\t{variableName} = {{");
                    foreach (var vnet in env.VirtualNetworks)
                    {
                        builder.AppendLine($"\t\t{vnet.Key} = {{");
                        builder.AppendLine($"\t\t\taddress_space = \"{vnet.Value.AddressSpace}\"");
                        builder.AppendLine($"\t\t\tsubnets = {{");
                        foreach (var subnet in vnet.Value.Subnets)
                        {
                            builder.AppendLine($"\t\t\t\t{subnet.Key} = {{");
                            builder.AppendLine($"\t\t\t\t\taddress_space = \"{subnet.Value.AddressSpace}\"");
                            builder.AppendLine("\t\t\t\t}");
                        }
                        builder.AppendLine("\t\t\t}");
                        builder.AppendLine("\t\t}");
                    }
                    builder.AppendLine("\t}");
                    builder.AppendLine("}");
                    
                    File.WriteAllText(tfFile, builder.ToString());
                }
                catch (Exception ex)
                {
                    PrintException(ex);
                }
            }, stateFileLocation, variableName);
            rootCommand.Add(generateTfCommand);
        }
        
        await rootCommand.InvokeAsync(args);
    }
}
