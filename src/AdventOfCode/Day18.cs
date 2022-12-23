namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day18 : BaseDay
{
    private readonly string _input;

    public Day18()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var cubes = _input.Split(Environment.NewLine).Select(l => l.Split(','))
            .Select(x => new Cube(int.Parse(x[0]), int.Parse(x[1]), int.Parse(x[2])));

        var connectedSides = 0;
        foreach (var cube in cubes)
        {
            foreach (var other in cubes.Where(c => c != cube))
            {
                if (Math.Abs(cube.X - other.X) + Math.Abs(cube.Y - other.Y) + Math.Abs(cube.Z - other.Z) == 1) { connectedSides++; }
            }
        }

        var result = cubes.Count() * 6 - connectedSides;

        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var space = new LavaSpace(_input);
        var result = space.CountExteriorSides();
        return new(result.ToString());

    }
}

public sealed class LavaSpace
{
    private Cube[] LavaCubes { get; }
    private int MaxZ { get; }
    private int MaxX { get; }
    private int MaxY { get; }
    private int MinZ { get; }
    private int MinX { get; }
    private int MinY { get; }
    public LavaSpace(string input)
    {
        var cubes = input.Split(Environment.NewLine).Select(l => l.Split(','))
        .Select(x => new Cube(int.Parse(x[0]), int.Parse(x[1]), int.Parse(x[2])));

        LavaCubes = cubes.ToArray();
        MaxZ = LavaCubes.Max(cube => cube.Z);
        MaxY = LavaCubes.Max(cube => cube.Y);
        MaxX = LavaCubes.Max(cube => cube.X);
        MinZ = LavaCubes.Min(cube => cube.Z);
        MinY = LavaCubes.Min(cube => cube.Y);
        MinX = LavaCubes.Min(cube => cube.X);
    }

    
    // flood fill
    public int CountExteriorSides()
    {
        var exteriorSides = 0;

        var visited = new HashSet<Cube>();
        var queue = new Queue<Cube>();
        queue.Enqueue(new Cube(MinX, MinY, MinZ));
        while (queue.Count > 0)
        {
            var cube = queue.Dequeue();
            visited.Add(cube);

            // Console.WriteLine($"At cube {cube.X} {cube.Y} {cube.Z}");
            var candidates = GetAdjacentCandidateCubes(cube);
            exteriorSides += CountLavaCubes(candidates);


            GetAdjacentUnvisitedNonLavaCubes(cube, visited, queue).ForEach(c => queue.Enqueue(c));
        }

        return exteriorSides;
    }

    private static List<Cube> GetAdjacentCandidateCubes(Cube cube) => new()
    {
        new Cube(cube.X + 1, cube.Y, cube.Z),
        new Cube(cube.X - 1, cube.Y, cube.Z),
        new Cube(cube.X, cube.Y + 1, cube.Z),
        new Cube(cube.X, cube.Y - 1, cube.Z),
        new Cube(cube.X, cube.Y, cube.Z + 1),
        new Cube(cube.X, cube.Y, cube.Z - 1),
    };

    private int CountLavaCubes(List<Cube> cubes)
    {
        int count = 0;
        foreach (var c in cubes)
        {
            if (LavaCubes.Contains(c))
            {
                // Console.WriteLine($" - {c.X} {c.Y} {c.Z} is lava, exterior sides +1");
                count++;
            }
        }
        return count;
    }

    private List<Cube> GetAdjacentUnvisitedNonLavaCubes(Cube cube, HashSet<Cube> visited, Queue<Cube> queue)
    {
        var newCoords = GetAdjacentCandidateCubes(cube);

        var list = new List<Cube>();

        foreach (var c in newCoords)
        {
            // add 1 to min and max to go around the cubes that are on the edges of space
            if (c.X < MinX - 1 || c.X > MaxX + 1 || c.Y < MinY - 1 || c.Y > MaxY + 1 || c.Z < MinZ - 1 || c.Z > MaxZ + 1) continue;
            if (visited.Contains(c) || queue.Contains(c) || LavaCubes.Contains(c)) continue;
            list.Add(c);
        }

        return list;
    }
}

public sealed class Cube
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }


    public Cube(int x, int y, int z)
    {
        X = x; Y = y; Z = z;
    }

    public override string ToString()
    {
        return $"{X}, {Y}, {Z}";
    }

    public override bool Equals(object obj)
    {
        if (obj is Cube other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hashCode = 1861411795;
        hashCode = hashCode * -1521134295 + X.GetHashCode();
        hashCode = hashCode * -1521134295 + Y.GetHashCode();
        hashCode = hashCode * -1521134295 + Z.GetHashCode();
        return hashCode;
    }
}