namespace AdventOfCode;

public class Day03 : BaseDay
{
    private readonly IEnumerable<Rucksack> _rucksacks;

    public Day03()
    {
        var input = File.ReadAllText(InputFilePath);
        var lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        _rucksacks = lines.Select(x => new Rucksack(x));

    }

    public override ValueTask<string> Solve_1() {
        var prioritiesOfDuplicateItems = _rucksacks.Sum(r => r.GetDuplicateItemPriority());
        return new (prioritiesOfDuplicateItems.ToString());
    }

    public override ValueTask<string> Solve_2()
        {
            var elfGroups = _rucksacks.Chunk(3);
            var prioritiesOfCommonItems = elfGroups.Sum(g => Rucksack.GetPriorityOfCommonItemInRucksacks(g));
            return new ValueTask<string>(prioritiesOfCommonItems.ToString());
        }
}

public class Rucksack
{
    private readonly string _compartment1;
    private readonly string _compartment2;
    private readonly string _allContent;


    public Rucksack(string ruckSackInputLine) {
        var compartmentSize = ruckSackInputLine.Length / 2;
        _compartment1 = ruckSackInputLine[..compartmentSize];
        _compartment2 = ruckSackInputLine[compartmentSize..];
        _allContent = ruckSackInputLine;
    }

    public int GetDuplicateItemPriority() => GetItemPriority(FindDuplicateItem());

    private char FindDuplicateItem() => _compartment1.Intersect(_compartment2).First();

    private static int GetItemPriority(char item)
    {
        var itemAsciiNo = (int)item;
        var priority = (itemAsciiNo < 97 ? itemAsciiNo - 64 + 26 : itemAsciiNo - 96);
        return priority;
    }

    public static int GetPriorityOfCommonItemInRucksacks(Rucksack[] rucksacks)
    {
        var commonItem = FindCommonItemInRucksacks(rucksacks);
        return GetItemPriority(commonItem);
    }

    private static char FindCommonItemInRucksacks(Rucksack[] rucksacks)
    {
        var firstRucksack = rucksacks[0];
        IEnumerable<char> commonItems = firstRucksack._allContent;
        foreach (var rucksack in rucksacks[1..])
        {
            commonItems = commonItems.Intersect(rucksack._allContent);
        }

        return commonItems.First();
    }
}