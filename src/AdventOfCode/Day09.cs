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
        var rope = new Rope();
        var moves = _input.Split(Environment.NewLine).Select(i => new Move(i));

        foreach (var move in moves) rope.DoMove(move);

        var result = rope.VisitedTailPositions.Count();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var result = 2;
        return new(result.ToString());
    }
}

public class Rope
{
    public Position HeadPosition { get; set; }
    public Position TailPosition { get; set; }
    public HashSet<Position> VisitedTailPositions = new HashSet<Position>();

    public Rope()
    {
        HeadPosition = new Position(0, 0);
        TailPosition= new Position(0, 0);
        VisitedTailPositions.Add(TailPosition);
    }

    public void DoMove(Move move)
    {
        for (int i = 0; i < move.Steps; i++)
        {
            Step(move.Direction);
            VisitedTailPositions.Add(TailPosition);
        }
    }

    public void Step(Direction direction)
    {
        HeadPosition = HeadPosition.GetAdjacentPosition(direction);
        if (TailPosition.IsAdjacentOrOverlapping(HeadPosition)) return;

        TailPosition = TailPosition.GetClosestAdjacentPosition(HeadPosition);
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

        var xDistance = target.X - X;
        var yDistance = target.Y - Y;

        int newX, newY;

        if (Math.Abs(xDistance) > Math.Abs(yDistance))
        {
            newY = target.Y;
            if (target.X == X) newX = X;
            else if (target.X > X) newX = target.X - 1;
            else newX = target.X + 1;
        }

        else
        {
            newX = target.X;
            if (target.Y == Y) newY = Y;
            else if (target.Y > Y) newY = target.Y - 1;
            else newY = target.Y + 1;
        }

        return new Position(newX, newY);
    }

}