namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day25 : BaseDay
{
    private readonly string _input;

    public Day25()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var snafus = SnafuCalculator.ParseInput(_input);
        var result = SnafuCalculator.SumAll(snafus);
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        return new("No part 2 :)".ToString());
    }
}

public sealed class SnafuCalculator
{
    public static Snafu[] ParseInput(string input)
        => input.Split(Environment.NewLine)
            .Select(x => new Snafu(x.ToCharArray())).ToArray();

    public static Snafu SumAll(Snafu[] snafus)
    {
        return snafus.Aggregate(new Snafu(), (sum, snafu) => sum + snafu);
    }
}

public readonly struct Snafu
{
    public readonly char[] Digits;

    public Snafu()
    {
        Digits = Array.Empty<char>();
    }

    public Snafu(char[] digits)
    {
        Digits = digits;
    }

    public static Snafu operator +(Snafu a, Snafu b)
    {
        var result = new List<char>();

        var longer = a.Digits.Length > b.Digits.Length ? a : b;
        var shorter = a.Digits.Length > b.Digits.Length ? b : a;

        var carryover = 0;
        char sum;
        for (var i = 0; i < shorter.Digits.Length; i++)
        {
            var fromEndIndex = i + 1;
            (sum, carryover) = AddOneDigit(longer.Digits[^fromEndIndex], shorter.Digits[^fromEndIndex], carryover);
            result.Insert(i, sum);
        }

        for (var i = shorter.Digits.Length; i < longer.Digits.Length; i++)
        {
            var fromEndIndex = i + 1;
            var digit = longer.Digits[^fromEndIndex];
            (sum, carryover) = carryover != 0 ? AddOneDigit(longer.Digits[^fromEndIndex], '0', carryover) : (digit, 0);
            result.Insert(i, sum);
        }

        if (carryover != 0)
        {
            result.Insert(longer.Digits.Length, IntToDigit(carryover));
        }

        result.Reverse();
        return new Snafu(result.ToArray());
    }

    private static (char digit, int carryover) AddOneDigit(char aDigit, char bDigit, int previousCarryover = 0)
    {
        var a = DigitToInt(aDigit);
        var b = DigitToInt(bDigit);

        var intSum = a + b + previousCarryover;
        var carryover = 0;
        switch (intSum)
        {
            case < -2:
                carryover = -1;
                intSum += 5;
                break;
            case > 2:
                carryover = 1;
                intSum -= 5;
                break;
        }

        return (IntToDigit(intSum), carryover);
    }

    private static int DigitToInt(char d) => d switch
    {
        '=' => -2,
        '-' => -1,
        '0' => 0,
        '1' => 1,
        '2' => 2,
        _ => throw new NotSupportedException()
    };

    private static char IntToDigit(int i) => i switch
    {
        2 => '2',
        1 => '1',
        0 => '0',
        -1 => '-',
        -2 => '=',
        _ => throw new NotSupportedException()
    };

    public override string ToString() => new(Digits);
}
