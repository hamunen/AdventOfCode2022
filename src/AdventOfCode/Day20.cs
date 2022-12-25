
using System.Collections.Generic;

namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day20 : BaseDay
{
    private readonly string _input;

    public Day20()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var file = new EncryptedFile(_input);

        var myList = file.MixFile();
        Console.Clear();
        var othList = AltSol
            .Day20(_input.Split(Environment.NewLine).ToList());


        var result = file.FindGroveCoordinates();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        return new(2.ToString());
    }
}

public sealed class EncryptedFile
{
    public int[] Original { get; set; }
    public EncryptedFile(string input)
    {
        Original = input.Split(Environment.NewLine).Select(line => int.Parse(line)).ToArray();
    }

    public int FindGroveCoordinates()
    {
        var mixed = MixFile();
        var max = mixed.Max();
        var min = mixed.Min();

        var result = FindNthNumberAfterZero(mixed, 1000)
            + FindNthNumberAfterZero(mixed, 2000)
            + FindNthNumberAfterZero(mixed, 3000);

        return result;
    }

    public int[] MixFile()
    {
        var list = Original.Select(x => (value: x, visited: false)).ToList(); // 2nd vlaue = visited
        int i = 0;
        while (i < Original.Length)
        {
            if (!list[i].visited) MoveValue(list, i);
            if (list[i].visited) i++;
        }

        return list.Select(l => l.value).ToArray();
    }

    public static void MoveValue(List<(int value, bool visited)> list, int index)
    {
        Console.Write($"index: ");
        var value = list[index].value;
        Console.Write($"value {value}");

        if (value == 0) { list[index] = (0, true); return; }
        var newIndex = index + value; // -1 for negative?

        list.RemoveAt(index);


        if (newIndex >= list.Count) newIndex %=list.Count;
        else if (newIndex <= 0)
            newIndex = list.Count + (newIndex % list.Count);
        
        //Console.Write($", goes between {list[newIndex].value}");
        //Console.WriteLine($" and {list[newIndex + 1 == 5000 ? 0 : newIndex + 1].value}");

        list.Insert(newIndex, (value, true));
    }

    public static int FindNthNumberAfterZero(int[] numbers, int n)
    {
        var zeroIndex = Array.IndexOf(numbers, 0);
        var newIndex = (zeroIndex + n) % numbers.Length;
        return numbers[newIndex];
    }
}


public sealed class AltSol
{
    public static int[] Day20(List<string> input)
    {
        var list = new LinkedList<int>();
        var list2 = new LinkedList<long>();
        var nodes = new List<LinkedListNode<int>>();
        var nodes2 = new List<LinkedListNode<long>>();

        const long key = 811589153;

        for (int i = 0; i < input.Count; i++)
        {
            nodes.Add(list.AddLast(int.Parse(input[i])));
            nodes2.Add(list2.AddLast(long.Parse(input[i]) * key));
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            int moves = node.Value;
            Console.Write($"index: value {moves}");

            if (moves < 0)
            {
                LinkedListNode<int> swap;
                for (int j = 0; j < -moves; j++)
                {
                    swap = node.Previous ?? node.List.Last;
                    list.Remove(node);
                    list.AddBefore(swap, node);
                }
            }
            else if (moves > 0)
            {
                for (int j = 0; j < moves; j++)
                {
                    var swap = node.Next ?? node.List.First;
                    list.Remove(node);
                    list.AddAfter(swap, node);
                }
            }

            Console.Write($", goes between {node.Previous.Value}");
            var andval = node.Next == null ? node.List.First.Value : node.Next.Value;
            Console.WriteLine($" and {andval}");

        }

        return nodes.Select(n => n.Value).ToArray();

        var search = nodes.First(node => node.Value == 0);
        int nums = 0;
        for (int i = 0; i <= 3000; i++)
        {
            if (i == 1000)
            {
                nums += search.Value;
            }
            else if (i == 2000)
            {
                nums += search.Value;
            }
            else if (i == 3000)
            {
                nums += search.Value;
                break;
            }
            search = search.Next ?? search.List.First;
        }

        Console.WriteLine($"Part 1: {nums}");

        for (int r = 0; r < 10; r++)
        {
            for (int i = 0; i < nodes2.Count; i++)
            {
                var node = nodes2[i];
                long moves = node.Value % (list2.Count - 1);
                if (moves < 0)
                {
                    for (int j = 0; j < -moves; j++)
                    {
                        var swap = node.Previous ?? node.List.Last;
                        list2.Remove(node);
                        list2.AddBefore(swap, node);
                    }
                }
                else if (moves > 0)
                {
                    for (int j = 0; j < moves; j++)
                    {
                        var swap = node.Next ?? node.List.First;
                        list2.Remove(node);
                        list2.AddAfter(swap, node);
                    }
                }
            }
        }

        var search2 = nodes2.First(node => node.Value == 0);
        long nums2 = 0;
        for (int i = 0; i <= 3000; i++)
        {
            if (i == 1000)
            {
                nums2 += search2.Value;
            }
            else if (i == 2000)
            {
                nums2 += search2.Value;
            }
            else if (i == 3000)
            {
                nums2 += search2.Value;
                break;
            }
            search2 = search2.Next ?? search2.List.First;
        }

        Console.WriteLine($"Part 2: {nums2}");
    }
}