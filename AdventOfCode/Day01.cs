namespace AdventOfCode;

public class Day01 : BaseDay
{
    private readonly string _input;
    private readonly string _result;
    private readonly string _result2;


    public Day01()
    {
        _input = File.ReadAllText(InputFilePath);

        // put into methods??

        var elves = _input.Split(new string[] { Environment.NewLine + Environment.NewLine },
                               StringSplitOptions.RemoveEmptyEntries);

        var elvesCalories = elves.Select(e => e.Split(Environment.NewLine).Select(Int32.Parse));

        var mostCalories = elvesCalories.Max(e => e.Sum());
        _result = mostCalories.ToString();

        // 2
        var topThreeCaloryCarryingElves = elvesCalories
            .OrderByDescending(e => e.Sum())
            .Take(3)
            .Sum(e => e.Sum());

        _result2 = topThreeCaloryCarryingElves.ToString();
    }

    public override ValueTask<string> Solve_1() => new(_result);
 
    public override ValueTask<string> Solve_2() => new(_result2);
}
