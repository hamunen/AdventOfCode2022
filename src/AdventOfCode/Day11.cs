using Microsoft.Extensions.Logging;

namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day11 : BaseDay
{
    private readonly string _input;
    
    // TODO: implement logging?
    // private readonly ILogger _logger;

    public Day11()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var business = new MonkeyBusiness(_input);
        business.Run(20);
        var result = business.GetLevelOfMonkeyBusiness();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var business = new MonkeyBusiness(_input, false);
        business.Run(10000);
        var result = business.GetLevelOfMonkeyBusiness();

        var test = business.Monkeys
            .OrderByDescending(m => m.InspectCounter);


        return new(result.ToString());
    }
}

public sealed class MonkeyBusiness
{
    public List<Monkey> Monkeys { get; set; }
    private int LCM { get; set; }

    public MonkeyBusiness(string input) : this(input, true) { }

    public MonkeyBusiness(string input, bool noDamageCausesRelief) {
        var monkeyInputs = input
            .Split(Environment.NewLine + Environment.NewLine)
            .Select(x => x.Trim());

        Monkeys = new List<Monkey>();

        foreach (var monkeyInput in monkeyInputs)
        {
            Monkeys.Add(Monkey.ParseFromInput(monkeyInput, noDamageCausesRelief));
        }

        // I don't like having to pass this around...
        LCM = Calculator.CalculateLCM(Monkeys.Select(m => m.TestDivisor));
        foreach (var monkey in Monkeys) { monkey.WorrylevelDivisor = LCM; }
    }

    public long GetLevelOfMonkeyBusiness() =>
            Monkeys
            .Select(m => (long)m.InspectCounter)
            .OrderByDescending(c => c)
            .Take(2)
            .Aggregate((c1, c2) => c1 * c2);

    public void Run(int rounds)
    {
        for (int i = 1; i <= rounds; i++)
        {
            foreach (var monkey in Monkeys)
            {
                monkey.InspectAndThrowitems(Monkeys);
            }
        }
    }
}

// probably better to create separate "No relief" monkey class...
public sealed class Monkey
{
    public int Index { get; set; }
    public Queue<int> Items { get; set; }
    public Func<long, long> Operation { get; set; } 
    public int MonkeyToThrowToIfTestTrue { get; set; }
    public int MonkeyToThrowToIfTestFalse { get; set; }
    public int InspectCounter { get; set; } = 0;
    private bool NoDamageCausesRelief { get; set; }
    public int TestDivisor { get; set; }
    public int WorrylevelDivisor { get; set; }


    public static Monkey ParseFromInput(string input, bool noDamageCausesRelief)
    {
        var lines = input.Trim().Split(Environment.NewLine);
        var monkey = new Monkey
        {
            Index = Int32.Parse(lines[0][7].ToString()),
            Items = ParseStartingItemsLine(lines[1]),
            Operation = ParseOperationLine(lines[2]),
            TestDivisor = ParseTestLine(lines[3]),
            MonkeyToThrowToIfTestTrue = ParseThrowLine(lines[4]),
            MonkeyToThrowToIfTestFalse = ParseThrowLine(lines[5]),
            NoDamageCausesRelief = noDamageCausesRelief
        };

        return monkey;
    }

    // does it make sense to pass all monkeys like this?
    // in a sense I guess - a monkey does know all other monkeys in the business...
    public void InspectAndThrowitems(List<Monkey> monkeys)
    {
        while (Items.Count > 0)
        {
            var item = Items.Dequeue();
            InspectAndThrowItem(item, monkeys);
        }
    }

    public void InspectAndThrowItem(int worryLevel, List<Monkey> monkeys)
    {
        var newWorryLevel = Operation(worryLevel);

        if (NoDamageCausesRelief)
        {
            newWorryLevel /= 3;
        } 
        else
        {
            // protect from overflow by moduloing with
            // lowest common multiplier of test divisors
            newWorryLevel %= WorrylevelDivisor;
        }

        var throwToIndex = Test((int)newWorryLevel)
            ? MonkeyToThrowToIfTestTrue
            : MonkeyToThrowToIfTestFalse;
        monkeys[throwToIndex].ThrowItemTo((int)newWorryLevel);
        InspectCounter++;
    }

    public void ThrowItemTo(int worryLevel) => Items.Enqueue(worryLevel);

    private static Queue<int> ParseStartingItemsLine(string input) {
        var items = new Queue<int>();

        input[(input.IndexOf(":") + 2)..].Trim()
            .Split(",")
            .Select(x => Int32.Parse(x.Trim())).ToList()
            .ForEach(i => items.Enqueue(i));

        return items;
    }

    private static Func<long, long> ParseOperationLine(string input)
    {
        var operationIndex = input.IndexOf("=")+2;
        var operationElements = input[operationIndex..].Trim()
            .Split(" ")
            .Select(x => x.Trim()).ToArray();

        if (operationElements[0] != "old") 
            throw new InvalidDataException("Expected old as first element");

        var operand = operationElements[2];

        if (operationElements[1] == "+")
            return OperationFactory.CreateAdder(Int32.Parse(operand));

        // if (operationElements[1] == "*")
        if (operand == "old")
            return OperationFactory.CreateSelfMultiplier();

        return OperationFactory.CreateMultiplier(Int32.Parse(operand));
     }

    private static int ParseTestLine(string input)
    {
        var divisor = Int32.Parse(
            input[(input.IndexOf("by") + 3)..].Trim());
        return divisor;
    }

    private static int ParseThrowLine(string input)
        => Int32.Parse(input.Split(" ").Last().Trim());

    public bool Test(int worryLevel) => worryLevel % TestDivisor == 0;
}

public sealed class OperationFactory
{
    public static Func<long, long> CreateAdder(int amountToAdd) 
        => x => x + amountToAdd;

    public static Func<long, long> CreateMultiplier(int multiplier) 
        => x => x * multiplier;

    public static Func<long, long> CreateSelfMultiplier()
        => (x) => x * x;
}

public sealed class Calculator
{
    public static int CalculateLCM(IEnumerable<int> numbers)
    {
        return numbers.Aggregate(Lcm);
    }
    private static int Lcm(int a, int b)
    {
        return Math.Abs(a * b) / GCD(a, b);
    }
    private static int GCD(int a, int b)
    {
        return b == 0 ? a : GCD(b, a % b);
    }
}