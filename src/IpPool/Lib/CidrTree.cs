using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Rendering;

#pragma warning disable CS8629 // Nullable value type may be null.
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace IpPool.Lib;

public class CidrTrie
{
    private TrieNode _root;
    private int _rootMask;
    private readonly IpAddr _rootIp;
    
    public IpAddr RootIp => _rootIp;
    
    public CidrTrie(IpAddr rootIp)
    {
        if (!rootIp.PrefixSize.HasValue)
        {
            throw new ArgumentOutOfRangeException(nameof(rootIp));
        }
        _rootIp = rootIp;
        _root = new TrieNode(0, 32 - _rootIp.PrefixSize.Value);
    }
    
    public IpAddr AllocateCidr(int size)
    {
        if (size > (32 - _rootIp.PrefixSize))
        {
            throw new BusinessException("requested size is too big");
        }

        TrieNode Walk(TrieNode node)
        {
            if (node.IsReserved)
            {
                return null;
            }

            if (node.MaskSize == size)
            {
                return node;
            }

            if (node.MaskSize == 0)
            {
                return null;
            }
            
            if (!node.Children.ContainsKey(0))
            {
                node.Children[0] = new TrieNode(0, node.MaskSize - 1)
                {
                    Parent = node
                };
            }

            var result = Walk(node.Children[0]);

            if (result == null)
            {
                if (!node.Children.ContainsKey(1))
                {
                    node.Children[1] = new TrieNode(1, node.MaskSize - 1)
                    {
                        Parent = node
                    };
                }

                result = Walk(node.Children[1]);
            }

            return result;
        }

        var found = Walk(_root);

        if (found != null)
        {
            found.IsReserved = true;
            var current = found;
            var bits = new List<byte>();
            while (current != null && current.Parent != null)
            {
                bits.Add(current.Bit);
                current = current.Parent;
            }

            uint reservedIp = 0;
            for (var x = bits.Count - 1; x >= 0; x--)
            {
                reservedIp = (reservedIp << 1) | bits[x];
            }

            var t = Convert.ToString(reservedIp, 2).PadLeft(32, '0');
            Console.WriteLine(Convert.ToString(reservedIp, 2).PadLeft(32, '0'));
            
            reservedIp <<= ((32 - _rootIp.PrefixSize.Value) - bits.Count);
            t = Convert.ToString(reservedIp, 2).PadLeft(32, '0');
            Console.WriteLine(t);
            return new IpAddr(_rootIp.Value | reservedIp, 32 - size);
        }

        throw new InvalidOperationException();
    }
    
    private UInt32 BitReverse(UInt32 value)
    {
        UInt32 left = (UInt32)1 << 31;
        UInt32 right = 1;
        UInt32 result = 0;

        for (int i = 31; i >= 1; i -= 2)
        {
            result |= (value & left) >> i;
            result |= (value & right) << i;
            left >>= 1;
            right <<= 1;
        }
        return result;
    }

    public string DebugOutput()
    {
        using var writer = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out = new AnsiConsoleOutput(writer)
        });
        
        // Create the tree
        var root = new Tree(string.Empty);

        void Walk(IHasTreeNodes tree, TrieNode node)
        {
            var childNode = tree.AddNode($"bit: {node.Bit}, reserved: {node.IsReserved}, size: {node.MaskSize}");
            foreach (var child in node.Children)
            {
                Walk(childNode, child.Value);
            }
        }
        
        
        Walk(root, _root);
        
        console.Write(root);

        return writer.ToString();
    }
}