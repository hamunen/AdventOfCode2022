using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;


namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day13 : BaseDay
{
    private readonly string _input;

    public Day13()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var device = new HandheldDevice(_input);
        var result = device.PacketPairs
            .Where(p => p.IsInRightOrder())
            .Sum(p => p.PairIndex);
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var sorter = new PacketSorter(_input);
        var result = sorter.GetDecoderKey();
        return new(result.ToString());
    }
}

public sealed class HandheldDevice
{
    public List<PairOfPackets> PacketPairs { get; set; }

    public HandheldDevice(string input)
    {
        PacketPairs = new List<PairOfPackets>();
        ParseInput(input);
    }

    private void ParseInput(string input)
    {
        var inputPairs = input.Split(Environment.NewLine + Environment.NewLine);
        var counter = 1;

        foreach (var inputPair in inputPairs)
        {
            var lines = inputPair.Split(Environment.NewLine);
            var pair = new PairOfPackets
            {
                PacketOne = new Packet(lines[0]),
                PacketTwo = new Packet(lines[1]),
                PairIndex = counter
            };

            PacketPairs.Add(pair);
            counter++;
        }
    }
}

public sealed class PacketSorter
{
    public List<Packet> Packets { get; set; }

    public PacketSorter(string input)
    {
        Packets = new List<Packet>();
        ParseInput(input);
        AddDividerLines();
    }

    private void ParseInput(string input)
    {
        var lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.ToList().ForEach(line => Packets.Add(new Packet(line)));
    }

    private void AddDividerLines()
    {
        Packets.Add(Packet.CreateDividerPacket("[[2]]"));
        Packets.Add(Packet.CreateDividerPacket("[[6]]"));
    }

    public IEnumerable<Packet> OrderPackets() => Packets.Order();

    public int GetDecoderKey()
    {
        var orderedPackets = OrderPackets().ToList();
        var dividers = orderedPackets.Where(p => p.IsDivider);

        var index1 = orderedPackets.IndexOf(dividers.ElementAt(0)) + 1;
        var index2 = orderedPackets.IndexOf(dividers.ElementAt(1)) + 1;

        return index1 * index2;
    }
}

public sealed class PairOfPackets
{
    public Packet PacketOne { get; set; }
    public Packet PacketTwo { get; set; }
    public int PairIndex { get; set; }


    public override string ToString()
    {
        return PacketOne.ToString() + " --- " +  PacketTwo.ToString();
    }

    public bool IsInRightOrder()
    {
        return PacketOne.CompareTo(PacketTwo) < 0;
    } 
}

public sealed class Packet : IComparable<Packet>
{
    public List<PacketElement> Elements { get; set; }
    public bool IsDivider { get; set; }

    public Packet(string input)
    {
        Elements = new List<PacketElement>();
        ParseInput(input);
    }

    public static Packet CreateDividerPacket(string input)
    {
        var packet = new Packet(input);
        packet.IsDivider = true;
        return packet;
    }

    public void ParseInput(string input)
    {
        dynamic array = JsonSerializer.Deserialize<dynamic>(input);
        Elements = ParseArray(array);
    }

    public static List<PacketElement> ParseArray(dynamic array)
    {
        var list = new List<PacketElement>();
        foreach (var element in array.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                int value = JsonSerializer.Deserialize<int>(element);
                list.Add(new PacketElement(value));
            }
            else if (element is JsonElement)
            {
                JsonElement nestedElement = (JsonElement)element;
                var parsed = ParseArray(nestedElement);
                list.Add(new PacketElement(parsed));
            }
            else throw new InvalidDataException("Something wrong in the input");
        }

        return list;
    }

    public override string ToString()
    {
        var str = "";
        foreach (var el in Elements)
        {
            str += el.ToString();
        }

        return str;
    }

    public PacketElement GetAsPacketElement()
    {
        return new PacketElement(Elements);
    }

    public int CompareTo(Packet other)
    {
        var left = this.GetAsPacketElement();
        var right = other.GetAsPacketElement();

        return AreElementsInRightOrder(left, right) != Order.Wrong ? -1 : 1;
    }

    private enum Order
    {
        Right,
        Wrong,
        Undecided
    }

    private static Order AreElementsInRightOrder(PacketElement left, PacketElement right)
    {
        if (!left.HasInnerEelements && !right.HasInnerEelements)
        {
            if (left.IntValue < right.IntValue) return Order.Right;
            if (left.IntValue > right.IntValue) return Order.Wrong;
            return Order.Undecided;
        }

        var leftElements = left.GetElements();
        var rightElements = right.GetElements();

        for (int i = 0; i < leftElements.Count; i++)
        {
            if (i >= rightElements.Count) return Order.Wrong; // right side runs out of elements first
            var innerOrder = AreElementsInRightOrder(leftElements[i], rightElements[i]);
            if (innerOrder != Order.Undecided) return innerOrder;
        }

        // left runs out before right
        if (leftElements.Count < rightElements.Count) return Order.Right;

        // I guess if we reach the end and are undecided, it's good
        return Order.Undecided;
    }
}

public class PacketElement
{
    public int IntValue { get; set; }
    public List<PacketElement> InnerElements { get; set; }
    public bool HasInnerEelements { get; set; } = false;

    public PacketElement(int intValue)
    {
        IntValue = intValue;
    }

    public PacketElement(List<PacketElement> innerElements)
    {
        InnerElements = innerElements;
        HasInnerEelements = true;
    }

    public List<PacketElement> GetElements()
    {
        if (!HasInnerEelements) return new List<PacketElement>() { new PacketElement(IntValue) };
        return InnerElements;
    }

    public override string ToString()
    {
        if (!HasInnerEelements)
        {
            return IntValue.ToString();
        }
        var str = "[";
        foreach (var el in InnerElements)
        {
            str += el.ToString();
        }
        str += "]";

        return str;
    }
}


