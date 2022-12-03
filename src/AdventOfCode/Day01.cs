namespace AdventOfCode;

public class Day01 : BaseDay
{
    private readonly IEnumerable<IEnumerable<int>> _elvesCalories;

    public Day01()
    {
        var input = File.ReadAllText(InputFilePath);
        var elves = input.Split(new string[] { Environment.NewLine + Environment.NewLine },
                               StringSplitOptions.RemoveEmptyEntries);
        _elvesCalories = elves.Select(e => e.Split(Environment.NewLine).Select(Int32.Parse));
    }

    public override ValueTask<string> Solve_1() {
        var mostCalories = _elvesCalories.Max(e => e.Sum());
        return new ValueTask<string>(mostCalories.ToString());
        }

public override ValueTask<string> Solve_2()
    {
        var caloriesOfTopThreeElves = _elvesCalories
           .OrderByDescending(e => e.Sum())
           .Take(3)
           .Sum(e => e.Sum());
        return new ValueTask<string>(caloriesOfTopThreeElves.ToString());
    }
}
