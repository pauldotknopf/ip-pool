using System.CommandLine;
using System.Text.Json;
using IpPool.Lib;

public class Program
{
    private static void SaveState(string stateFile, CidrState state)
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
    
    private static CidrTrie LoadState(string stateFile)
    {
        if (!File.Exists(stateFile))
        {
            throw new BusinessException($"state file {stateFile} doesn't exist");
        }

        var state = JsonSerializer.Deserialize<CidrState>(File.ReadAllText(stateFile),
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        return CidrTrie.FromState(state);
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
                    var cidr = new CidrTrie(new IpAddr(poolValue));
                    SaveState(stateFileValue, cidr.GetState());
                }
                catch (Exception ex)
                {
                    PrintException(ex);
                }
            }, stateFileLocation, pool);
            rootCommand.Add(createPoolCommand);
        }
        
        {
            var size = new Option<int>
                (name: "--size",
                description: "the size of the CIDR to reserve")
            {
                IsRequired = true
            };
            var key = new Option<string>
                (name: "--key",
                description: "the key to use for the reservation")
            {
                IsRequired = true
            };
            
            var reserveSizeCommand = new Command("reserve-size");
            reserveSizeCommand.AddOption(stateFileLocation);
            reserveSizeCommand.AddOption(size);
            reserveSizeCommand.AddOption(key);
            reserveSizeCommand.SetHandler((stateFileValue, sizeValue, keyValue) =>
            {
                try
                {
                    var pool = LoadState(stateFileValue);
                    var reserved = pool.AllocateCidr(sizeValue, keyValue);
                    Console.WriteLine("reserved: " + reserved);
                    SaveState(stateFileValue, pool.GetState());
                }
                catch (Exception ex)
                {
                    PrintException(ex);
                }
            }, stateFileLocation, size, key);
            rootCommand.Add(reserveSizeCommand);
        }

        {
            var ip = new Option<string>
                (name: "--ip",
                description: "the ip (in CIDR notation) to reserve")
            {
                IsRequired = true
            };
            var key = new Option<string>
                (name: "--key",
                description: "the key to use for the reservation")
            {
                IsRequired = true
            };
            
            var reserveIpCommand = new Command("reserve-ip");
            reserveIpCommand.AddOption(stateFileLocation);
            reserveIpCommand.AddOption(ip);
            reserveIpCommand.AddOption(key);
            reserveIpCommand.SetHandler((stateFileValue, ipValue, keyValue) =>
            {
                try
                {
                    var pool = LoadState(stateFileValue);
                    var reserved = pool.AllocateCidr(new IpAddr(ipValue), keyValue);
                    Console.WriteLine("reserved: " + reserved);
                    SaveState(stateFileValue, pool.GetState());
                }
                catch (Exception ex)
                {
                    PrintException(ex);
                }
            }, stateFileLocation, ip, key);
            rootCommand.Add(reserveIpCommand);
        }
        
        await rootCommand.InvokeAsync(args);
    }
}
