
using System.Text;

namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day24 : BaseDay
{
    private readonly string _input;

    public Day24()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var basin = new BlizzardBasin(_input);
        var result = basin.GetShortestPathStartToTarget();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var basin = new BlizzardBasin(_input);
        var result = basin.GetShortestPathStartToTargetAndBackAndBack();
        return new(result.ToString());
    }
}

// Should be cleaned and simplified :)
// lots of unnecessary stuff left hanigng around

public sealed class BlizzardBasin
{
    public (int x, int y) StartingPosition { get; set; }
    public static (int x, int y) Target { get; set; }

    public BlizzardCell[,] StartingBlizzards { get; set; }
    private int _maxY;
    private int _maxX;
    private readonly BlizzardCell[] _movementFlags = new BlizzardCell[4] { BlizzardCell.Up, BlizzardCell.Down, BlizzardCell.Left, BlizzardCell.Right };
    //private readonly BlizzardCell[][,] _blizzardPositions;
    private readonly HashSet<(int x, int y)>[] _safePositions;
    private readonly int _blizzardRotation;

    public BlizzardBasin(string input)
    {
        StartingPosition = (0, -1);
        ParseInput(input);
        //_blizzardPositions = PopulateBlizzardPositions();
        _safePositions = PopulateBlizzardPositions();
        _blizzardRotation = _safePositions.Length;
    }

    public static bool SequenceEquals<TBlizzardCell>(TBlizzardCell[,] a, TBlizzardCell[,] b) => 
        a.Cast<TBlizzardCell>().SequenceEqual(b.Cast<TBlizzardCell>());

    public int GetShortestPathStartToTarget() => ShortestPath(StartingPosition, Target, 0);

    public int GetShortestPathStartToTargetAndBackAndBack()
    {
        var minutes1 = ShortestPath(StartingPosition, Target, 0);
        var minutes2 = ShortestPath(Target, StartingPosition, minutes1);
        var minutes3 = ShortestPath(StartingPosition, Target, minutes2);

        return minutes3;
    }

    public int ShortestPath((int x, int y) startPos, (int x, int y) targetPos, int startMinute)
    {
        var start = new Position(startPos, startMinute, targetPos);
        var best = int.MaxValue;

       var visited = new HashSet<Position>();
       var unvisited = new PriorityQueue<Position, int>();
        unvisited.Enqueue(start, 0);

        while (unvisited.Count != 0)
        {

            var position = unvisited.Dequeue();
            var pos = position.Pos;
            var minute = position.Minute;

            if (minute + position.DistanceToTarget >= best) continue;

            // wow! chaning this from a list with comparisons to a hashset/dictionary daramatically increased performance
            if (visited.Contains(position))
                continue;
            visited.Add(position);

            if (pos.y == targetPos.y && pos.x == targetPos.x)
            {
                best = minute;
                continue;
            }

            var safePositionsNextMinute = _safePositions[(minute + 1) % _blizzardRotation];
            var possibleMoves = GetPossibleMovements(pos.x, pos.y, safePositionsNextMinute);
            foreach (var newPos in possibleMoves.Select(move => new Position(move, minute + 1, targetPos)))
            {
                unvisited.Enqueue(newPos, newPos.DistanceToTarget * 1000 + newPos.Minute );
            }
        }
        return best;
    }

    private record struct Position
    {
        public (int x, int y) Pos { get; }
        public int DistanceToTarget { get; }
        public int Minute { get; }
        public Position((int x, int y) pos, int minute, (int x, int y) target) { 
            Pos = pos; 
            Minute = minute;
            DistanceToTarget = Math.Abs(target.x - pos.x) + Math.Abs(target.y - pos.y);
        }

    }

    private List<(int x, int y)> GetPossibleMovements(int x, int y, HashSet<(int x, int y)> safePositions)
    {
        var list = new List<(int x, int y)>();
        if (y == -1) // starting pos - down or wait are only options
        {
            list.Add((x, y)); // Wait
            if (safePositions.Contains((x, y + 1))) list.Add((x, y + 1)); // Down
            return list;
        }
        // one below starting pos - can move up
        if (x == 0 && y == 0) list.Add((0, -1));

        // target pos - up or wait
        if (y == Target.y && x == Target.x)
        {
            list.Add((x, y)); // Wait
            if (safePositions.Contains((x, y - 1))) list.Add((x, y - 1)); // Up
            return list;
        }
        if (y == Target.y - 1 && x == Target.x)
        {
            list.Add((x, y + 1)); // Down
        }

        if (y < _maxY && safePositions.Contains((x, y + 1))) list.Add((x, y + 1)); // Down
        if (safePositions.Contains((x, y))) list.Add((x, y)); // Wait
        if (y > 0 && safePositions.Contains((x, y - 1))) list.Add((x, y - 1)); // Up
        if (x > 0 && safePositions.Contains((x - 1, y))) list.Add((x - 1, y)); // Left
        if (x < _maxX && safePositions.Contains((x + 1, y))) list.Add((x + 1, y)); // Right

        // empty list = no movements, you're dead
        return list;
    }

    private BlizzardCell[,] AdvanceMinute(BlizzardCell[,] current)
    {
        var newPositions = new BlizzardCell[_maxX + 1, _maxY + 1];

        for (var y = 0; y <= _maxY; y++)
        {
            for (var x = 0; x <= _maxX; x++)
            {
                var cell = current[x, y];
                foreach (var flag in _movementFlags.Where(f => cell.HasFlag(f)))
                {
                    var newPos = GetNewPos(x, y, flag); 
                    newPositions[newPos.x, newPos.y] |= flag;
                }
            }
        }

        return newPositions;
    }

    private HashSet<(int x, int y)> ConvertToSafePositionMap(BlizzardCell[,] blizzards)
    {
        var safePositions = new HashSet<(int x, int y)>();
        for (var y = 0; y <= _maxY; y++)
        {
            for (var x = 0; x <= _maxX; x++)
            {
                if (blizzards[x, y] == BlizzardCell.None)
                    safePositions.Add((x, y));
            }
        }

        return safePositions;
    }

    private (int x, int y) GetNewPos(int x, int y, BlizzardCell cell) =>  
        cell switch
        {
            BlizzardCell.Up => y == 0 ? (x, _maxY) : (x, y - 1),
            BlizzardCell.Down => y == _maxY ? (x, 0) : (x, y + 1),
            BlizzardCell.Left => x == 0 ? (_maxX, y) : (x - 1, y),
            BlizzardCell.Right => x == _maxX ? (0, y) : (x + 1, y),
            _ => throw new Exception()
        };

    public HashSet<(int x, int y)>[] PopulateBlizzardPositions()
    {
        var blizzardPositions = new List<BlizzardCell[,]>();
        var safePositions = new List<HashSet<(int x, int y)>>();
        var current = StartingBlizzards;
        while (true)
        {
            blizzardPositions.Add(current);
            safePositions.Add(ConvertToSafePositionMap(current));
            current = AdvanceMinute(current);

            if (blizzardPositions.Any(p => SequenceEquals(p, current)))
                break;
        }

        //return blizzardPositions.ToArray();
        return safePositions.ToArray();
    }

    private void ParseInput(string input)
    {
        var lines = input.Split(Environment.NewLine);
        _maxX = lines[0].Length - 3;
        _maxY = lines.Length - 3;

        Target = (lines[^1].IndexOf('.') - 1, _maxY + 1);
        StartingBlizzards = new BlizzardCell[_maxX + 1, _maxY + 1];

        for (int y = 0; y <= _maxY; y++)
        {
            for (int x = 0; x <= _maxX; x++)
            {
                StartingBlizzards[x,y] = lines[y+1][x+1] switch
                {
                    '>' => BlizzardCell.Right,
                    'v' => BlizzardCell.Down,
                    '<' => BlizzardCell.Left,
                    '^' => BlizzardCell.Up,
                    '.' => BlizzardCell.None,
                    _ => throw new InvalidDataException()
                };
            }
        }

        //BlizzardCells[0, 0] = BlizzardCell.None | BlizzardCell.Left;
    }

    public void Draw()
    {
        var builder = new StringBuilder();

        for (int y = 0; y <= _maxY; y++)
        {
            for (int x = 0; x <= _maxX; x++)
            {
                var cell = StartingBlizzards[x, y];
                var flags = _movementFlags.Where(f => cell.HasFlag(f)).ToList();
                if (flags.Count > 1)
                {
                    builder.Append(flags.Count);
                }
                else if (flags.Count == 0)
                {
                    builder.Append('.');
                }
                else
                {
                    var ch = flags[0] switch
                    {
                        BlizzardCell.Right => '>',
                        BlizzardCell.Down => 'v',
                        BlizzardCell.Left => '<',
                        BlizzardCell.Up => '^',
                        BlizzardCell.None => '.',
                        _ => throw new InvalidDataException()
                    };
                    builder.Append(ch);
                }

            }
            builder.Append(Environment.NewLine);
        }

        Console.WriteLine(builder.ToString());
    }
}

[Flags]
public enum BlizzardCell
{
    None = 0x0000,
    Up = 0x0001,
    Down = 0x0010,
    Left = 0x0100,
    Right = 0x1000,
}