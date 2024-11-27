namespace IpPool.Lib;

public class IpAddr
{
    public int? PrefixSize { get; set; }
    
    public uint Value { get; set; }
    
    public IpAddr(uint value)
    {
        Value = value;
    }
    
    public IpAddr(uint value, int prefix)
    {
        Value = value;
        if(prefix < 0 || prefix > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        PrefixSize = prefix;
    }
    
    public IpAddr(string value)
    {
        string ip;
        if (value.Contains("/"))
        {
            var parts = value.Split('/');
            ip = parts[0];
            var size = parts[1];
            if (string.IsNullOrEmpty(size))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            if(!int.TryParse(size.Trim(), out var parsedSize))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            if(parsedSize < 0 || parsedSize > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            PrefixSize = parsedSize;
        }
        else
        {
            ip = value;
        }

        if (string.IsNullOrEmpty(ip))
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        ip = ip.Trim();
        
        var ipParts = ip.Split('.');

        if (ipParts.Length != 4)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
        
        uint ipInt = 0;
        foreach (var part in ipParts)
        {
            ipInt = (ipInt << 8) | uint.Parse(part);
        }
        
        Value = ipInt;
        if (PrefixSize.HasValue)
        {
            uint t = 0;
            var rightBitsToWipe = 32 - PrefixSize.Value;
            while (rightBitsToWipe > 0)
            {
                t = (t << 1) | 1;
                rightBitsToWipe--;
            }

            Value &= (~t);
        }
    }

    public override string ToString()
    {
        var ip = string.Format("{0}.{1}.{2}.{3}",
            (Value >> 24) & 0xFF,
            (Value >> 16) & 0xFF,
            (Value >> 8) & 0xFF,
            Value & 0xFF);
        if (PrefixSize.HasValue)
        {
            ip += $"/{PrefixSize}";
        }

        return ip;
    }
}