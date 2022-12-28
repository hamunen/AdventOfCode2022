
namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day23 : BaseDay
{
    private readonly string _input;

    public Day23()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var grove = new AshGrove(_input);
        grove.PlayRounds(10);
        var result = grove.CalculateEmptyGroundTiles();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var grove = new AshGrove(_input);
        var result = grove.PlayRoundsUntilNoElvesMove();
        return new(result.ToString());
    }
}

// i think i'll try just listing the elves positions, not the map
public sealed class AshGrove
{
    public Elf[] Elves { get; set; }
    public DirectionGenerator DirectionGenerator { get; set; }

    public AshGrove(string input)
    {
        Elves = ParseInput(input);
        DirectionGenerator = new DirectionGenerator();
    }

    public void PlayRounds(int rounds)
    {
        //Draw();

        for (int i = 0; i < rounds; i++ )
        {
            PlayRound();
            // Draw();
        }
    }

    public int PlayRoundsUntilNoElvesMove()
    {
        //Draw();

        var elvesMoved = true;
        var rounds = 0;
        while (elvesMoved)
        {
            elvesMoved = PlayRound();
            rounds++;
            // Draw();
        }
        return rounds;
    }

    private void Draw()
    {
        var minX = Elves.Min(e => e.X);
        var maxX = Elves.Max(e => e.X);
        var minY = Elves.Min(e => e.Y);
        var maxY = Elves.Max(e => e.Y);

        var str = "";
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (Elves.Any(e => e.X == x && e.Y == y))
                {
                    str += '#';
                } else
                {
                    str += '.';
                }
            }
            str += Environment.NewLine;
        }

        Console.WriteLine();
        Console.WriteLine(str);
    }

    public int CalculateEmptyGroundTiles()
    {
        var minX = Elves.Min(e => e.X);
        var maxX = Elves.Max(e => e.X);
        var minY = Elves.Min(e => e.Y);
        var maxY = Elves.Max(e => e.Y);

        var area = (maxX - minX + 1) * (maxY - minY + 1);
        return area - Elves.Length;
    }

    public bool PlayRound()
    {
        var dirs = DirectionGenerator.GetNextDirections();
        var elvesConsideringToMove = new List<Elf>();
        foreach (var elf in Elves)
        {
            var freeDirs = new List<GroveDirection>();
            foreach (var dir in dirs)
            {
                if (IsFreeToMoveToDirection(elf, dir)) freeDirs.Add(dir);
            }

            // all surrounding spots are free, or all directions are blocked -> stay
            if (freeDirs.Count == 4) continue;
            if (freeDirs.Count == 0) continue;

            elf.ConsiderMove(freeDirs.First());
            elvesConsideringToMove.Add(elf);
        }

        foreach (var elf in elvesConsideringToMove)
        {
            // same considered pos, but not the same elf :)
            // can we only consider elves considering to move? I think so since if not moving, it wouldn't be in the list
            if (!elvesConsideringToMove.Any(e => elf.ConsideredX == e.ConsideredX && elf.ConsideredY == e.ConsideredY && !(e.X == elf.X && e.Y == elf.Y)))
                elf.DoConsideredMove();
        }

        return elvesConsideringToMove.Count > 0;
     }

    public bool IsFreeToMoveToDirection(Elf thisElf, GroveDirection dir) => dir switch
    {
        GroveDirection.North => !Elves.Any(e => e.Y == thisElf.Y - 1 && e.X <= thisElf.X + 1 && e.X >= thisElf.X - 1),
        GroveDirection.South => !Elves.Any(e => e.Y == thisElf.Y + 1 && e.X <= thisElf.X + 1 && e.X >= thisElf.X - 1),
        GroveDirection.West => !Elves.Any(e => e.Y <= thisElf.Y + 1 && e.Y >= thisElf.Y - 1 && e.X == thisElf.X - 1),
        GroveDirection.East => !Elves.Any(e => e.Y <= thisElf.Y + 1 && e.Y >= thisElf.Y - 1 && e.X == thisElf.X + 1),
        _ => throw new NotImplementedException()
    };


    private static Elf[] ParseInput(string input)
    {
        var elves = new List<Elf>();
        var lines = input.Split(Environment.NewLine);
        for (int y = 0; y < lines.Length; y++) { 
            for (int x = 0; x < lines[0].Length; x++) {
                if (lines[y][x] == '#') elves.Add(new Elf(x, y));
            }
        }

        return elves.ToArray();
    }
}

public sealed class Elf
{
    public int X { get; set; }
    public int Y { get; set; }

    public int ConsideredX { get; set; }
    public int ConsideredY { get; set; }

    public Elf(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void ConsiderMove(GroveDirection dir)
    {
        switch (dir)
        {
            case GroveDirection.North:
                SetConsider(X, Y - 1);
                break;
            case GroveDirection.South:
                SetConsider(X, Y + 1);
                break;
            case GroveDirection.West:
                SetConsider(X - 1, Y);
                break;
            case GroveDirection.East:
                SetConsider(X + 1, Y);
                break;
        }
    }

    public void ConsiderToStay()
    {
        SetConsider(X, Y);
    }

    public void SetConsider(int x, int y)
    {
        ConsideredX = x;
        ConsideredY = y;
    }

    public void DoConsideredMove()
    {
        X = ConsideredX; Y = ConsideredY;
    }
}

public sealed class DirectionGenerator
{
    private GroveDirection[] _directions = new GroveDirection[] { GroveDirection.North, GroveDirection.South, GroveDirection.West, GroveDirection.East };
    private int _currentStartIndex = 0;

    public GroveDirection[] GetNextDirections()
    {
        var dirs = new GroveDirection[4];
        for (int i = 0; i < 4; i++)
        {
            var index = (_currentStartIndex + i) % 4;
            dirs[i] = _directions[index];
        }

        _currentStartIndex = (_currentStartIndex + 1) % 4;
        return dirs;
    }
}

public enum GroveDirection
{
    North, South, West, East
}
