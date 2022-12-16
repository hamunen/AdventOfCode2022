using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day14 : BaseDay
{
    private readonly string _input;

    public Day14()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var cave = Cave.ParseInput(_input);
        // uncomment to draw to console!
        // cave.Draw(); 

        
        var result = cave.LetAllSandPourUntilHitsTheFloowAndCalculateUnitsOfSand();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var cave = Cave.ParseInput(_input);
        cave.HasFloor = true;
        // uncomment to draw to console!
        // cave.Draw(); 


        var result = cave.LetAllSandPourUntilHitsTheFloowAndCalculateUnitsOfSand();
        return new(result.ToString());
    }
}

public sealed class Cave
{
    private readonly int _maxX;
    private readonly int _maxY;
    public char[,] Map { get; set; }
    public Dictionary<(int X, int Y), char> Grid { get; set; }
    public List<List<char>> Coordinates { get; set; }
    private (int x, int y) _sandSource = (500, 0);
    private bool _finished = false;
    public bool HasFloor { get; set; } = false;
    private int _floorY;

    public Cave(int maxX, int maxY)
    {
        _maxX = maxX;
        _maxY = maxY;
        _floorY = maxY + 2;
        Map = new char[maxX + 1, maxY + 1];
        Grid = new();
        Grid[_sandSource] = '+';
    }

    public static Cave ParseInput(string input)
    {
        var lines = input.Split(Environment.NewLine);
        var rocks = new List<Coordinate>();


        foreach (var line in lines)
        {
            rocks.AddRange(ParseLine(line));
        }

        var cave = new Cave(rocks.Max(r => r.X), rocks.Max(r => r.Y));
        foreach (var coord in rocks)
        {
            cave.Map[coord.X, coord.Y] = '#';
            cave.Grid[(coord.X, coord.Y)] = '#';
        }

        return cave;
    }

    public static List<Coordinate> ParseLine(string input)
    {
        var rocks = new List<Coordinate>();
        var coordinates = input.Split(" -> ").Select(c => c.Split(',')).ToArray();
        for (var i = 1; i < coordinates.Length; i++)
        {
            var x1 = int.Parse(coordinates[i][0]);
            var y1 = int.Parse(coordinates[i][1]);
            var x0 = int.Parse(coordinates[i - 1][0]);
            var y0 = int.Parse(coordinates[i - 1][1]);

            rocks.AddRange(CreateRockLine(
                Math.Min(x0, x1), Math.Min(y0, y1),
                Math.Max(x0, x1), Math.Max(y0, y1)));
        }

        return rocks;
    }

    public static List<Coordinate> CreateRockLine(int startX, int startY, int endX, int endY)
    {
        var rocks = new List<Coordinate>();
        for (int x = startX; x <= endX; x++) { 
            for (int y = startY; y <= endY; y++)
            {
                rocks.Add(new Coordinate(x, y));
            }
        }

        return rocks;
    }

    public void DropSand()
    {
        MoveSand(_sandSource.x, _sandSource.y);
    }

    public void MoveSand(int x, int y)
    {
        if (!HasFloor && (y > _maxY || x > _maxX || _finished))
        {
            Grid[(x, y + 2)] = 'X';
            _finished = true;
            return;
        }

        if (HasFloor && y == _floorY - 1)
        {
            Grid[(x, y)] = 'o';
            return;
        }


        if (!Grid.ContainsKey((x, y + 1))) { MoveSand(x, y + 1); return; }
        if (!Grid.ContainsKey((x-1, y + 1))) { MoveSand(x-1, y + 1); return; }
        if (!Grid.ContainsKey((x + 1, y + 1))) { MoveSand(x + 1, y + 1); return; }
        
        Grid[(x, y)] = 'o';

        if (y == _sandSource.y)
        {
            _finished = true;
            return;
        }
    }

    public int LetAllSandPourUntilHitsTheFloowAndCalculateUnitsOfSand()
    {
        while (!_finished) DropSand();
        return Grid.Where(p => p.Value == 'o').Count();
    }

    public void Draw()
    {
        //ApplicationConfiguration.Initialize();
        //Application.Run(new AocForm(Grid));

        ReDraw();

        while (true)
        {
            DropSand();
            // draw again until any other key than space is pressed
            var k = Console.ReadKey().Key;
            if (k == ConsoleKey.X)
            {
                for (int i = 0; i < 10; i++) { DropSand(); }
            }
            else if (k != ConsoleKey.Spacebar) break;
            ReDraw();
        }

    }

    public void ReDraw()
    {
        var minX = Grid.Keys.Select(k => k.X).Min();
        var maxX = Grid.Keys.Select(k => k.X).Max();

        var minY = Grid.Keys.Select(k => k.Y).Min();
        //var maxY = Grid.Keys.Select(k => k.Y).Max();
        var maxYWithSand = Grid.Where(p => p.Value == 'o').DefaultIfEmpty().Max(p => p.Key.Y) ;

        var maxY = Math.Max(50, maxYWithSand) + 5;

        var map = new StringBuilder();
        for (var y = minY; y <= maxY; y++)
        {
            var line = new StringBuilder();
            for (var x = minX; x <= maxX; x++)
            {
                if (Grid.ContainsKey((x, y)))
                {
                    line.Append(Grid[(x, y)]);
                } else if (y == _floorY)
                {
                    line.Append('-');
                }
                else
                {
                    line.Append('.');
                }
            }
            map.AppendLine(line.ToString());
        }

        Console.Clear();
        Console.Write(map);
        Console.WriteLine("space to drop 1 sand, x to drop 10 sands, any other key to quit");
    }


}

public sealed class Coordinate
{
    public int X { get; set; }
    public int Y { get; set; }
    //public Element Element { get; set; }

    public Coordinate(int x, int y) { X = x; Y = y; }
}


public enum Element
{
    Rock = '#',
    Air = ' ',
    Sand = 'o'
}