namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day09 : BaseDay
{
    private string _input;
    public Day09()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var rope = new Rope(2);
        var moves = _input.Split(Environment.NewLine).Select(i => new Move(i));

        foreach (var move in moves) rope.DoMove(move);

        var result = rope.VisitedTailPositions.Count;
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var rope = new Rope(10);
        var moves = _input.Split(Environment.NewLine).Select(i => new Move(i));

        foreach (var move in moves)
        {
            rope.DoMove(move);
        }

        var result = rope.VisitedTailPositions.Count;
        return new(result.ToString());
    }
}

public class Rope
{
    public Position[] Knots { get; set; }
    public Position HeadPosition => Knots.First();
    public Position TailPosition => Knots.Last();

    public HashSet<Position> VisitedTailPositions = new HashSet<Position>();
    private int MoveCounter { get; set; } = 0;

    public Rope(int knots)
    {
        Knots = new Position[knots];
        for (int i = 0; i < knots; i++)
        {
            Knots[i] = new Position(0, 0);
        }

        VisitedTailPositions.Add(TailPosition);
    }

    public void VisualizeView()
    {
        DataGridView grid = new DataGridView();

    }

    public string Visualize()
    {
        char[,] grid = new char[60, 60];
        for (int i = 0; i < 60; i++)
        {
            for (int j = 0; j < 60; j++)
            {
                grid[i, j] = ' ';
            }
        }

       foreach (var pos in VisitedTailPositions)
        {
            grid[pos.X + 30, pos.Y + 30] = '#';
        }

        grid[HeadPosition.X + 30, HeadPosition.Y + 30] = 'H';
        for (int i = 1; i < Knots.Length - 1; i++)
        {
            var knot = Knots[i];
            grid[knot.X + 30, knot.Y + 30] = Char.Parse(i.ToString());
        }
        grid[TailPosition.X + 30, TailPosition.Y + 30] = 'T';

        var output = "";
        for (int j = 59; j >= 0; j--)
        {
            for (int i = 0; i < 60; i++)
            {
                output+= grid[i, j];
            } output += "\n";
        }

        return output;
    }

    public void DoMove(Move move)
    {
        MoveCounter++;
        for (int i = 0; i < move.Steps; i++)
        {
            Step(move.Direction);

            if (!VisitedTailPositions.Contains(TailPosition) && Knots.Length > 3)
            {
                VisitedTailPositions.Add(TailPosition);
            }
            VisitedTailPositions.Add(TailPosition);
        }
    }

    public void Step(Direction direction)
    {

        Knots[0] = HeadPosition.GetAdjacentPosition(direction);

        for (var i = 1; i < Knots.Length; i++)
        {

            var knot = Knots[i];
            var previousKnot = Knots[i - 1];

            if (knot.IsAdjacentOrOverlapping(previousKnot)) break;
            Knots[i] = knot.GetClosestAdjacentPosition(previousKnot);

        }
    }

}

public class Move
{
    public Direction Direction { get; set; }
    public int Steps { get; set; }

    public Move(string input)
    {
        var codes = input.Split(' ');
        Direction = (Direction)codes[0][0];
        Steps = Int32.Parse(codes[1]);
    }
}

public enum Direction
{
    Up = 'U',
    Down = 'D',
    Left = 'L',
    Right = 'R'
}

public class Position
{
    public int X { get; set; }
    public int Y { get; set; }

    public Position(int x, int y) { X = x; Y = y; }

    public override bool Equals(object obj)
    {
        var other = obj as Position;
        if (other == null)
        {
            return false;
        }

        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode() => (X.GetHashCode() * 397) ^ Y.GetHashCode();

    public Position GetAdjacentPosition(Direction direction) =>
        direction switch
        {
            Direction.Up => new Position(X, Y + 1),
            Direction.Down => new Position(X, Y - 1),
            Direction.Left => new Position(X - 1, Y),
            Direction.Right => new Position(X + 1, Y),
            _ => null
        };

    public bool IsAdjacentOrOverlapping(Position other) =>
        Math.Abs(X - other.X) <= 1 && Math.Abs(Y - other.Y) <= 1;

    public Position GetClosestAdjacentPosition(Position target)
    {
        if (IsAdjacentOrOverlapping(target)) return this;

        var xDistance = Math.Abs(target.X - X);
        var yDistance = Math.Abs(target.Y - Y);

        int newX = X;
        int newY = Y;

        if (target.X > X) newX = X + 1;
        if (target.X < X) newX = X - 1;
        if (target.Y > Y) newY = Y + 1;
        if (target.Y < Y) newY = Y - 1;

        return new Position(newX, newY);
    }

}