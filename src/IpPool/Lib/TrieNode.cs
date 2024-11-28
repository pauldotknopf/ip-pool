namespace IpPool.Lib;

public class TrieNode
{
    public TrieNode(byte bit, int maskSize)
    {
        Bit = bit;
        MaskSize = maskSize;
    }
    
    public byte Bit { get; set; }
    
    public int MaskSize { get; set; }
    
    public TrieNode Parent { get; set; }
    
    public string Key { get; set; }
    
    public bool IsReserved { get; set; } = false;
    
    public Dictionary<byte, TrieNode> Children { get; set; } = new();
}