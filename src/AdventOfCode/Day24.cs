
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
        basin.Play();
        return new(2.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        return new(2.ToString());
    }
}

public sealed class BlizzardBasin
{
    public (int x, int y) StartingPosition { get; set; }
    public (int x, int y) Target { get; set; }
    public int[,] UpBlizzards { get; set; }
    public int[,] DownBlizzards { get; set; }
    public int[,] LeftBlizzards { get; set; }
    public int[,] RigthBlizzards { get; set; }

    // byte = 8 bits...
    public byte[,] Blizzards { get; set; }
    public BlizzardCell[,] BlizzardCells { get; set; }
    private int _maxY;
    private int _maxX;
    private BlizzardCell[] _movementFlags = new BlizzardCell[4] { BlizzardCell.Up, BlizzardCell.Down, BlizzardCell.Left, BlizzardCell.Right };

    public BlizzardBasin(string input)
    {
        StartingPosition = (1, -1);
        ParseInput(input);
    }

    public void Play()
    {
        BlizzardCells = AdvanceMinute();
    }

    private BlizzardCell[,] AdvanceMinute()
    {
        var newPositions = new BlizzardCell[_maxX + 1, _maxY + 1];

        for (int y = 0; y <= _maxY; y++)
        {
            for (int x = 0; x <= _maxX; x++)
            {
                var cell = BlizzardCells[x, y];
                foreach (BlizzardCell flag in _movementFlags.Where(f => cell.HasFlag(f)))
                {
                    var newPos = GetNewPos(x, y, flag); 
                    newPositions[newPos.x, newPos.y] |= flag; // does this work?
                }
            }
        }

        return newPositions;
    }

    private (int x, int y) GetNewPos(int x, int y, BlizzardCell cell)
    {
        switch (cell)
        {
            case BlizzardCell.Up:
                if (y == 0) return (x, _maxY);
                return (x, y - 1);
            case BlizzardCell.Down:
                if (y == _maxY) return (x, 0);
                return (x, y + 1);
            case BlizzardCell.Left:
                if (x == 0) return (_maxX, y);
                return (x - 1, y);
            case BlizzardCell.Right:
                if (x == _maxX) return (0, y);
                return (x + 1, y);
            default:
                throw new Exception();
        }
    }

    private void ParseInput(string input)
    {
        var lines = input.Split(Environment.NewLine);
        _maxX = lines[0].Length - 3;
        _maxY = lines.Length - 3;

        Target = (lines[^1].IndexOf('.') - 1, _maxY + 1);
        BlizzardCells = new BlizzardCell[_maxX + 1, _maxY + 1];

        for (int y = 0; y <= _maxY; y++)
        {
            for (int x = 0; x <= _maxX; x++)
            {
                BlizzardCells[x,y] = lines[y+1][x+1] switch
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
}

[Flags]
public enum BlizzardCell
{
    None = 0x0000,
    Up = 0x0001,
    Down = 0x0010,
    Left = 0x0100,
    Right = 0x1000
}

// not needed?
public struct BasinCoordinate
{
    public int X { get; set; }
    public int Y { get; set; }

    public BasinCoordinate(int x, int y) { X = x; Y = y; }
}