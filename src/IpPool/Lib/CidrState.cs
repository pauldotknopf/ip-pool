namespace IpPool.Lib;

public class CidrState
{
    public string Pool { get; set; }
    
    public Dictionary<string, string> Reserved { get; set; }
}