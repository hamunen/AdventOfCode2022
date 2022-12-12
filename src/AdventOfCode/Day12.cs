
using Spectre.Console;
using System.ComponentModel;

namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day12 : BaseDay
{
    private readonly string _input;

    public Day12()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var map = HeightMap.ParseFromInput(_input);
        var pathFinder = new PathFinder(map);
        var result = pathFinder.GetShortestPathLength();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var map = HeightMap.ParseFromInput(_input);
        var pathFinder = new PathFinder(map);
        var result = pathFinder.GetShortestPathFromAnyAToEnd();
        return new(result.ToString());
    }
}

/* TODO: refactor
 * This solution is... not great and messy */

public sealed class HeightMap
{
    public char[,] Grid { get; set; }
    public Square Start { get; set; }
    public Square End { get; set; }

    public int MaxX => _maxX;
    public int MaxY => _maxY;

    private readonly int _maxX;
    private readonly int _maxY;


    public HeightMap(int xLength, int yLength) {
        Grid = new char[xLength, yLength];
        _maxX = xLength - 1;
        _maxY = yLength - 1;
    }

    public char GetElevation(Square square)
    {
        return Grid[square.X, square.Y];
    }

    public static HeightMap ParseFromInput(string input)
    {
        var lines = input.Split(Environment.NewLine);
        var yLength = lines.Length;
        var xLength = lines[0].Length;

        var map = new HeightMap(xLength, yLength);

        for (int y = 0; y < yLength; y++)
        {
            for (int x = 0; x < xLength; x++)
            {
                var marker = lines[yLength -1 - y][x];
                if (marker == 'S') {
                    marker = 'a';
                    map.Start = new Square(x, y);
                }
                else if (marker == 'E')
                {
                    marker = 'z';
                    map.End = new Square(x, y);
                }
                map.Grid[x, y] = marker;
            }
        }

        return map;
    }

    public List<Square> GetWalkableAdjacentSquares(Square square)
    {
        var possibleSquares = new List<Square>()
        {
            new Square(square.X, square.Y-1),
            new Square(square.X, square.Y+1),
            new Square(square.X-1, square.Y),
            new Square(square.X+1, square.Y),
        };

        return possibleSquares
            .Where(s => IsWalkableTo(square, s)).ToList();
    }

    public List<Square> GetReverseWalkableAdjacentSquares(Square square)
    {
        var possibleSquares = new List<Square>()
        {
            new Square(square.X, square.Y-1),
            new Square(square.X, square.Y+1),
            new Square(square.X-1, square.Y),
            new Square(square.X+1, square.Y),
        };

        return possibleSquares
            .Where(s => IsWalkableTo(s, square)).ToList();
    }

    public bool IsWalkableTo(Square from, Square destination)
    {
        if (destination.X < 0 || destination.X > MaxX) return false;
        if (destination.Y < 0 || destination.Y > MaxY) return false;
        if (from.X < 0 || from.X > MaxX) return false;
        if (from.Y < 0 || from.Y > MaxY) return false;

        var currentElevation = Grid[from.X, from.Y];
        var destinationElevation = Grid[destination.X, destination.Y];

        return destinationElevation - currentElevation <= 1;
    }
}

public sealed class PathFinder
{
    public List<PathFindingSquare> ActiveSquares { get; set; }
    public List<PathFindingSquare> VisitedSquares { get; set; }
    private PathFindingSquare EndSquare { get; set; }
    public HeightMap Map { get; set; }

    public PathFinder(HeightMap map) { 
        Map = map;
        ActiveSquares = new List<PathFindingSquare>();
        VisitedSquares = new List<PathFindingSquare>();

        ActiveSquares.Add(new PathFindingSquare(Map.Start, 
            Map.Start.GetDistanceTo(Map.End)));
    }

    private static int HeuristicScore(PathFindingSquare square)
    {
        // not sure why the elevation distance doesnt work well...
        // needs to be admissible heuristic?
        //return ('z' - Map.GetElevation(square));
        return square.DistanceToEnd;
    }

    // A* algorithm
    public void FindShortestPath()
    {
        while (ActiveSquares.Any())
        {
            var square = ActiveSquares
                .OrderBy(s => s.TravelsalCost + HeuristicScore(s))
                .First();

            //Console.WriteLine($"At square {square.X}, {square.Y}, dist {square.DistanceToEnd}, cost {square.TravelsalCost}");

            if (square.DistanceToEnd == 0)
            {
                EndSquare = square;
                return;
            }
            // x 18, y 17
            VisitedSquares.Add(square);
            ActiveSquares.Remove(square);

            var walkableSquares = Map
                .GetWalkableAdjacentSquares(square)
                .Select(s => new PathFindingSquare(s, square, s.GetDistanceTo(Map.End)));

            CheckWalkableSquares(walkableSquares);

            /*if (square.TravelsalCost > 0 && Map.GetElevation(square) > 'a')
            {
                VisualizePath(square);
                Console.WriteLine(square.ToString());
            }*/

        }

        if (EndSquare is null)
        {
            Console.WriteLine("Well shit");
            var lastSquare = VisitedSquares.Last();
            VisualizePath(lastSquare);
        }
    }

    private void VisualizePath(PathFindingSquare square)
    {
        var arr = Enumerable.Range(0, Map.MaxY + 1)
            .Select(x => Enumerable.Repeat(' ', Map.MaxX + 1).ToArray())
            .ToArray();

        var currentSquare = square;

        foreach (var visited in VisitedSquares)
        {
            arr[Map.MaxY - visited.Y][visited.X] = Map.Grid[visited.X, visited.Y];
        }

        foreach (var active in ActiveSquares)
        {
            arr[Map.MaxY - active.Y][active.X] = '*';
        }


        while (currentSquare is not null)
        {
            var elev = Map.Grid[currentSquare.X, currentSquare.Y];
            arr[Map.MaxY - currentSquare.Y][currentSquare.X] = char.ToUpper(elev);
            currentSquare = currentSquare.Parent;
        }

        var output = "";
        for (int y = 0; y <= Map.MaxY; y++)
        {
            for (int x = 0; x <= Map.MaxX; x++)
            {
                if (Map.Start.X == x && Map.Start.Y == y)
                {
                    output += "S";
                }
                else if (Map.End.X == x && Map.End.Y == y)
                {
                    output += "E";
                }
                else
                {
                    output += arr[y][x];
                }
            }
            output += Environment.NewLine;
        }

        Console.Clear();
        foreach (char c in output)
        {
            if (char.IsUpper(c))
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.Write(c);
        }
    }

    public int GetShortestPathLength()
    {
        FindShortestPath();
        // VisualizePath(EndSquare);
        return EndSquare.TravelsalCost;
    }

    public void CheckWalkableSquares(IEnumerable<PathFindingSquare> walkableSquares)
    {
        foreach (var square in walkableSquares)
        {
            if (VisitedSquares.Any(visited => visited.Equals(square))) continue;

            if (ActiveSquares.Any(active => active.Equals(square)))
            {
                var existingSquare = ActiveSquares.First(active => active.Equals(square));
                if (existingSquare.TravelsalCost + HeuristicScore(existingSquare) > square.TravelsalCost + HeuristicScore(existingSquare))
                {
                    ActiveSquares.Remove(existingSquare);
                    ActiveSquares.Add(square);
                }
            } else
            {
                ActiveSquares.Add(square);
            }
        }
    }

    public int GetShortestPathFromAnyAToEnd()
    {
        // dijkstra's algorithm, starting from end
        var unvisitedSquares = new List<HikingSquare>();
        var visitedSquares = new List<HikingSquare>();

        for (int i = 0; i <= Map.MaxX; i++)
        {
            for (int j = 0; j <= Map.MaxY; j++)
            {
                unvisitedSquares.Add(new HikingSquare(i, j));
            }
        }

        unvisitedSquares.First(s => s.Equals(Map.End)).ShortestDistanceFromStart = 0;

        while (unvisitedSquares.Any(s => s.ShortestDistanceFromStart != 999999))
        {
            var square = unvisitedSquares.OrderBy(s => s.ShortestDistanceFromStart).First();

            var adjacentSquares =
                Map.GetReverseWalkableAdjacentSquares(square).Select(s => new HikingSquare(s)).ToList();
            var unvisitedNeighbors = unvisitedSquares.Where(s => adjacentSquares.Contains(s));

            foreach (var neighbor in unvisitedNeighbors)
            {
                var distanceFromStart = square.ShortestDistanceFromStart + 1;
                if (distanceFromStart < neighbor.ShortestDistanceFromStart)
                {
                    neighbor.ShortestDistanceFromStart = distanceFromStart;
                    neighbor.Previous = square;
                }
            }

            visitedSquares.Add(square);
            unvisitedSquares.Remove(square);
        }
        var asd = visitedSquares
            .Where(s => Map.GetElevation(s) == 'a')
            .OrderBy(s => s.ShortestDistanceFromStart)
            .First();

        return asd.ShortestDistanceFromStart;
    }

}

public class Square
{
    public int X { get; set; }
    public int Y { get; set; }

    public Square(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"X: {X}, Y: {Y}";
    }

    public override bool Equals(object obj)
    {
        var other = obj as Square;
        if (other == null)
        {
            return false;
        }

        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode() => (X.GetHashCode() * 397) ^ Y.GetHashCode();

    public int GetDistanceTo(Square other) => 
        Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
}

public sealed class PathFindingSquare : Square
{
    public int TravelsalCost { get; set; }
    public PathFindingSquare Parent { get; set; }
    public int DistanceToEnd { get; set; }

    public PathFindingSquare(Square square, int distanceToEnd) : base(square.X, square.Y) {
        DistanceToEnd = distanceToEnd;
        TravelsalCost = 0;
    }
    public PathFindingSquare(Square square, PathFindingSquare parent, int distanceToEnd)
        : base(square.X, square.Y)
    {
        Parent = parent;
        DistanceToEnd = distanceToEnd;
        TravelsalCost = parent.TravelsalCost + 1;
    }

    public override string ToString()
    {
        return base.ToString();
    }
}

public sealed class HikingSquare : Square
{
    public int ShortestDistanceFromStart { get; set; }
    public HikingSquare Previous { get; set; }

    public HikingSquare(Square square) : base(square.X, square.Y) {
        ShortestDistanceFromStart = 999999;
    }

    public HikingSquare(int x, int y) : base(x, y) {
        ShortestDistanceFromStart = 999999;
    }
}