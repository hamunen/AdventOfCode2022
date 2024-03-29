﻿
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

        Console.Clear();
        var result = file.FindGroveCoordinates();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var file = new EncryptedFile(_input);

        var result = file.FindGroveCoordinatesWithKey();
        return new(result.ToString());
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

        var result = FindNthNumberAfterZero(mixed, 1000)
            + FindNthNumberAfterZero(mixed, 2000)
            + FindNthNumberAfterZero(mixed, 3000);

        return result;
    }

    public long FindGroveCoordinatesWithKey()
    {
        var mixed = MixFileWithKey();
        var result = FindNthNumberAfterZero(mixed, 1000)
            + FindNthNumberAfterZero(mixed, 2000)
            + FindNthNumberAfterZero(mixed, 3000);

        return result;
    }

    public long[] MixFileWithKey()
    {
        var list = Original.Select((x, i) => (value: ((long)x)*811589153L, originalIndex: i)).ToList();
        for (int k = 0; k < 10; k++) { 
            for (int i = 0; i < Original.Length; i++)
            {
                MoveValueLon(list, i);
            }
        }

        return list.Select(n => n.value).ToArray();
    }

    public static void MoveValueLon(List<(long value, int originalIndex)> list, int i)
    {
        var currentIndex = list.FindIndex(n => n.originalIndex == i);
        var value = list[currentIndex].value;
        if (value == 0) return;

        var newIndex = currentIndex + value;
        list.RemoveAt(currentIndex);

        if (newIndex >= list.Count) newIndex %= list.Count;
        if (newIndex <= 0)
            newIndex = list.Count + (newIndex % list.Count);

        list.Insert((int)newIndex, (value, i));
    }

    public int[] MixFile()
    {
        var list = Original.Select(x => (value: x, visited: false)).ToList();
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
        var newIndex = index + value;

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
    public static long FindNthNumberAfterZero(long[] numbers, int n)
    {
        var zeroIndex = Array.IndexOf(numbers, 0);
        var newIndex = (zeroIndex + n) % numbers.Length;
        return numbers[newIndex];
    }
}
