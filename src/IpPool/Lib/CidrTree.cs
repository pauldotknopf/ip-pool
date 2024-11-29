using System.Text.Json;
using Microsoft.VisualBasic;
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

    public static CidrTrie FromState(CidrState state)
    {
        var trie = new CidrTrie(new IpAddr(state.Pool));
        
        if (state.Reserved != null)
        {
            foreach (var reserved in state.Reserved)
            {
                trie.AllocateCidr(new IpAddr(reserved.Value), reserved.Key);
            }
        }
        
        return trie;
    }
    
    public IpAddr AllocateCidr(IpAddr ip, string key)
    {
        if (!ip.PrefixSize.HasValue)
        {
            throw new BusinessException($"prefix size is required: {ip}");
        }
        if(ip.PrefixSize.Value < _rootIp.PrefixSize.Value)
        {
            throw new BusinessException($"prefix size is too small: {ip}");
        }
        
        // Ensure the ip.Value prefix matches the prefix of _rootIp
        uint mask = uint.MaxValue << (32 - _rootIp.PrefixSize.Value);
        if ((ip.Value & mask) != (_rootIp.Value & mask))
        {
            throw new BusinessException($"IP prefix does not match the root IP prefix: {ip}");
        }

        var bitsToWalk = ip.PrefixSize - _rootIp.PrefixSize;
        
        var currentNode = _root;

        for (int i = 0; i < bitsToWalk; i++)
        {
            if (currentNode.IsReserved)
            {
                throw new Exception("already reserved");
            }
            uint bit = (ip.Value >> (31 - (_rootIp.PrefixSize.Value + i))) & 1;
            if (!currentNode.Children.ContainsKey((byte)bit))
            {
                currentNode.Children[(byte)bit] = new TrieNode((byte)bit, currentNode.MaskSize - 1)
                {
                    Parent = currentNode
                };
            }
            currentNode = currentNode.Children[(byte)bit];
        }

        var t = DebugOutput();

        void MakeSureNoChildrenAreReserved(TrieNode node)
        {
            if (node.IsReserved)
            {
                throw new BusinessException($"the requested reservation {ip} conflicts with {ToIp(node)}");
            }
            
            foreach (var child in node.Children)
            {
                MakeSureNoChildrenAreReserved(child.Value);
            }
        }

        MakeSureNoChildrenAreReserved(currentNode);
        
        currentNode.IsReserved = true;
        currentNode.Key = key;

        return ToIp(currentNode);
    }
    
    public IpAddr AllocateCidr(int size, string key)
    {
        if (size > (32 - _rootIp.PrefixSize))
        {
            throw new BusinessException("requested size is too big");
        }

        key = EnsureValidKey(key);
        
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
            found.Key = key;
            return ToIp(found);
        }

        throw new BusinessException("couldn't find a suitable CIDR block");
    }

    private string EnsureValidKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new BusinessException("key is required");
        }

        key = key.ToLower().Trim();

        string Walk(TrieNode node)
        {
            if (string.IsNullOrEmpty(node.Key))
            {
                foreach (var child in node.Children)
                {
                    var result = Walk(child.Value);
                    if (result != null)
                    {
                        return result;
                    }
                }

                return null;
            }
            
            if (node.Key.ToLower().Trim() == key)
            {
                return node.Key;
            }
            
            foreach (var child in node.Children)
            {
                var result = Walk(child.Value);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        var existing = Walk(_root);

        if (!string.IsNullOrEmpty(existing))
        {
            throw new BusinessException($"the key {key} is already in use");
        }

        return key;
    }

    private IpAddr ToIp(TrieNode node)
    {
        var current = node;
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

        reservedIp <<= ((32 - _rootIp.PrefixSize.Value) - bits.Count);
            
        return new IpAddr(_rootIp.Value | reservedIp, 32 - node.MaskSize);
    }

    public CidrState GetState()
    {
        var state = new CidrState
        {
            Pool = _rootIp.ToString(),
            Reserved = new Dictionary<string, string>()
        };

        void Walk(TrieNode node)
        {
            if (node.IsReserved)
            {
                state.Reserved.Add(node.Key, ToIp(node).ToString());
            }

            foreach (var child in node.Children)
            {
                Walk(child.Value);
            }
        }

        Walk(_root);

        return state;
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