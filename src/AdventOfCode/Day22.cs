
namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day22 : BaseDay
{
    private readonly string _input;

    public Day22()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    // Sorry. Solution is bad.

    public override ValueTask<string> Solve_1()
    {
        var groveMap = new GroveMap(_input);
        var result = groveMap.Move();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var groveCube = new GroveCube(_input);
        var result = groveCube.Move();
        return new(result.ToString());
    }
}

public sealed class GroveCube 
{
    public GroveMapTile[,] Map { get; set; }
    public List<Instruction> Instructions { get; set; }
    private GroveMap GroveMap { get; set; }

    public GroveCube(string input)
    {
        GroveMap = new GroveMap(input);
        Map = GroveMap.Map;
        Instructions = GroveMap.Instructions;
    }

    public int Move()
    {
        var currentPosition = GroveMap.FindFirstNonVoidTileFromLeft(0);
        var currentDirection = Direction.Up; // first turn is right
        var instructionIndex = 0;

        while (instructionIndex < Instructions.Count)
        {
            //Console.Clear();

            var printStr = $"Position: {currentPosition.x}, {currentPosition.y}, dir {currentDirection}";

            var currentInstruction = Instructions[instructionIndex];
            currentDirection = GroveMap.ChangeDirection(currentDirection, currentInstruction.Turn);
            (currentPosition, currentDirection) = StepForward(currentPosition, currentDirection, currentInstruction.Steps);
            instructionIndex++;

            printStr += $" - Instruction: {currentInstruction.Turn}, {currentInstruction.Steps}";
            printStr += $" - New pos: {currentPosition.x}, {currentPosition.y}, dir {currentDirection}";

            //Console.WriteLine(printStr);
            //GroveMap.DrawMap(currentPosition, currentDirection);
            //Console.ReadKey();
        }

        var dirValue = currentDirection switch
        {
            Direction.Right => 0,
            Direction.Down => 1,
            Direction.Left => 2,
            Direction.Up => 3,
            _ => throw new ArgumentOutOfRangeException()
        };
        return 1000 * (currentPosition.y + 1) + 4 * (currentPosition.x + 1) + dirValue;
    }

    private ((int x, int y) newPos, Direction newDir) StepForward((int x, int y) startPos, Direction startDir, int steps)
    {
        var pos = startPos;
        var dir = startDir;
        var str = "";
        for (int i = 0; i < steps; i++)
        {
            str += ".";
            var (newPos, newDir) = GetNextTile(pos, dir);
            if (Map[newPos.y, newPos.x] == GroveMapTile.Wall)
            {
                //Console.WriteLine(str + $"# at {newPos.x}, {newPos.y}");
                return (pos, dir);
            }
            pos = newPos; dir = newDir;
        }
        //Console.WriteLine(str + 'x');

        return (pos, dir);
    }

    private ((int x, int y) newPos, Direction newDir) GetNextTile((int x, int y) pos, Direction dir)
    {
        var currentSide = GetCurrentSide(pos);

        (int x, int y)newPos = dir switch
        {
            Direction.Up => (pos.x, pos.y - 1),
            Direction.Right => (pos.x + 1, pos.y),
            Direction.Down => (pos.x, pos.y + 1),
            Direction.Left => (pos.x - 1, pos.y),
            _ => throw new Exception()
        };

        var newDir = dir;

        // 1 - up
        if (currentSide == GroveCubeSide.Top && newPos.y < 0)// dir is up -> right
        {
            newPos = (0, (newPos.x - 50) + 150);
            newDir = Direction.Right;
        }
        // 1 - right , nothin changes
        // 1 - down, nothing changes
        // 1 - left
        else if (currentSide == GroveCubeSide.Top && newPos.x < 50)
        {
            newPos = (0, (49 - newPos.y) + 100);
            newDir = Direction.Right;
        }

        // 2 - up
        else if (currentSide == GroveCubeSide.East && newPos.y < 0)
        {
            newPos = (newPos.x - 100, 199);
            newDir = Direction.Up;
        }
        // 2 - right (to 5)
        else if (currentSide == GroveCubeSide.East && newPos.x > 149)
        {
            newPos = (99, (49 - newPos.y) + 100);
            newDir = Direction.Left;
        }
        // 2 - down (3 left)
        else if (currentSide == GroveCubeSide.East && newPos.y > 49)
        {
            newPos = (99, (newPos.x - 100) + 50);
            newDir = Direction.Left;
        }
        // 2 left, nothing changes
        // 3 up, ok
        // 3 right (2up)
        else if (currentSide == GroveCubeSide.South && newPos.x >= 100)
        {
            newPos = ((newPos.y - 50) + 100, 49);
            newDir = Direction.Up;
        }
        // 3 down, ok
        // 3 left (4 down)
        else if (currentSide == GroveCubeSide.South && newPos.x < 50)
        {
            newPos = (newPos.y - 50, 100);
            newDir = Direction.Down;
        }
        // 4 up (3 right)
        else if (currentSide == GroveCubeSide.West && newPos.y < 100)
        {
            newPos = (50, 50 + newPos.x); // fixed
            newDir = Direction.Right;
        }
        // 4 right, ok
        // 4 down, ok
        // 4 left (1 right reverse)
        else if (currentSide == GroveCubeSide.West && newPos.x < 0)
        {
            newPos = (50, (49 - (newPos.y - 100))); // e.g row 0 on 4 -> row 49 on 1
            newDir = Direction.Right;
        }
        // 5 up, ok
        // 5 right (2 left reverse)
        else if (currentSide == GroveCubeSide.Bottom && newPos.x >= 100)
        {
            newPos = (149, (49 - (newPos.y - 100))); // e.g row 0 on 4 -> row 49 on 1
            newDir = Direction.Left;
        }
        // 5 down (6 left)
        else if (currentSide == GroveCubeSide.Bottom && newPos.y >= 150)
        {
            newPos = (49, (newPos.x - 50) + 150);
            newDir = Direction.Left;
        }
        // 5 left, ok
        // 6 up, ok
        // 6 right (5 up)
        else if (currentSide == GroveCubeSide.North && newPos.x >= 50)
        {
            newPos = ((newPos.y - 150) + 50, 149);
            newDir = Direction.Up;
        }
        // 6 down (2 down)
        else if (currentSide == GroveCubeSide.North && newPos.y > 199)
        {
            newPos = (newPos.x + 100, 0);
            newDir = Direction.Down;
        }
        // 6 left (1 down)
        else if (newPos.x < 0 && currentSide == GroveCubeSide.North) 
        {
            newPos = ((newPos.y - 150) + 50, 0);
            newDir = Direction.Down;
        }

        // aika vaikee. ehkä parempi ois tehä ne 6 50x50 sideä ja siirtyä niiden välillä...

        return (newPos, newDir);
    }

    private static GroveCubeSide GetCurrentSide((int x, int y) pos)
    {
        return pos switch
        {
            ( < 100, < 50) => GroveCubeSide.Top,
            ( >= 100, < 50) => GroveCubeSide.East,
            ( > 0, < 100) => GroveCubeSide.South,
            ( <50 , < 150) => GroveCubeSide.West,
            ( >= 50, < 150) => GroveCubeSide.Bottom,
            _ => GroveCubeSide.North
        };
    }

}

public enum GroveCubeSide
{
    Top = 1,
    East = 2,
    South = 3,
    Bottom = 5,
    West = 4,
    North = 6
}

public class GroveMap
{
    public GroveMapTile[,] Map { get; set; }
    public List<Instruction> Instructions { get; set; }
    private int _height;
    private int _width;

    public GroveMap(string input)
    {
        var parts = input.Split(Environment.NewLine + Environment.NewLine);
        ParseMapInput(parts[0]);
        Instructions = ParseDirectionInput(parts[1]);
    }

    public int Move()
    {
        var currentPosition = FindFirstNonVoidTileFromLeft(0);
        var currentDirection = Direction.Up; // first turn is right
        var instructionIndex = 0;

        while (instructionIndex < Instructions.Count)
        {
            var printStr = $"Position: {currentPosition.x}, {currentPosition.y}, dir {currentDirection}";

            var currentInstruction = Instructions[instructionIndex];
            currentDirection = ChangeDirection(currentDirection, currentInstruction.Turn);
            currentPosition = StepForward(currentPosition, currentDirection, currentInstruction.Steps);
            instructionIndex++;

            printStr += $" - Instruction: {currentInstruction.Turn}, {currentInstruction.Steps}";
            printStr += $" - New pos: {currentPosition.x}, {currentPosition.y}, dir {currentDirection}";

             //Console.Clear();
            // DrawMap(currentPosition, currentDirection);
             //Console.WriteLine(printStr);

        }

        var dirValue = currentDirection switch
        {
            Direction.Right => 0,
            Direction.Down => 1,
            Direction.Left => 2,
            Direction.Up => 3,
            _ => throw new ArgumentOutOfRangeException()
        };
        return 1000 * (currentPosition.y + 1) + 4 * (currentPosition.x + 1) + dirValue;
    }

    public void DrawMap((int x, int y) pos, Direction dir)
    {
        var str = "";
        for (int row = 0; row < _height; row++)
        {
            for (int col = 0; col < _width; col++)
            {
                if (col == pos.x && row == pos.y)
                {
                    switch (dir)
                    {
                        case Direction.Up:
                            str += '^';
                            break;
                        case Direction.Down:
                            str += 'v';
                            break;
                        case Direction.Left:
                            str += '<';
                            break;
                        case Direction.Right:
                            str += '>';
                            break;
                    }
                }else
                {
                    str += ((char)Map[row, col]);
                }
            }
            str += Environment.NewLine;
        }

        //Console.WriteLine(str);
    }

    // x = col, y = row
    // Map = [y, x]
    // remember y is inverted

    private (int x, int y) StepForward((int x, int y) startPos, Direction dir, int steps)
    {
        var pos = startPos;
        for (int i = 0; i < steps; i++)
        {
            var nextTile = GetNextNonVoidTileInDirection(pos, dir);
            if (Map[nextTile.y, nextTile.x] == GroveMapTile.Wall) return pos;
            pos = nextTile;
        }

        return pos;
    }

    private (int x, int y) GetNextNonVoidTileInDirection((int x, int y) pos, Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                if (pos.y - 1 < 0 || Map[pos.y - 1, pos.x] == GroveMapTile.Void)
                    return FindFirstNonVoidTileFromBottom(pos.x);
                return (pos.x, pos.y-1);
            case Direction.Right:
                if (pos.x + 1 == _width || Map[pos.y, pos.x + 1] == GroveMapTile.Void)
                    return FindFirstNonVoidTileFromLeft(pos.y);
                return (pos.x+1, pos.y);
            case Direction.Down:
                if (pos.y + 1 == _height || Map[pos.y + 1, pos.x] == GroveMapTile.Void)
                    return FindFirstNonVoidTileFromTop(pos.x);
                return (pos.x, pos.y+1);
            case Direction.Left:
                if (pos.x - 1 < 0 || Map[pos.y, pos.x - 1] == GroveMapTile.Void)
                    return FindFirstNonVoidTileFromRight(pos.y);
                return (pos.x-1, pos.y);
        }

        throw new Exception("Shouldn't be here");
    }

    public static Direction ChangeDirection(Direction currentDirection, Turn turn)
    {
        if (turn == Turn.Left)
        {
            switch (currentDirection)
            {
                case Direction.Up:
                    return Direction.Left;
                case Direction.Right:
                    return Direction.Up;
                case Direction.Down:
                    return Direction.Right;
                case Direction.Left:
                    return Direction.Down;
            }
        }

        switch (currentDirection) // right
        {
            case Direction.Up:
                return Direction.Right;
            case Direction.Right:
                return Direction.Down;
            case Direction.Down:
                return Direction.Left;
            case Direction.Left:
                return Direction.Up;
        }

        throw new Exception("shouldn't be here");
    }

    public (int x, int y) FindFirstNonVoidTileFromTop(int col)
    {
        for (int row = 0; row < _height; row++)
        {
            if (Map[row, col] != GroveMapTile.Void) return (col, row);
        }
        throw new Exception();
    }

    public (int x, int y) FindFirstNonVoidTileFromBottom(int col)
    {
        for (int row = _height - 1; row >= 0; row--)
        {
            if (Map[row, col] != GroveMapTile.Void) return (col, row);
        }
        throw new Exception();
    }

    public (int x, int y) FindFirstNonVoidTileFromLeft(int row)
    {
        for (int col = 0; col < _width; col++)
        {
            if (Map[row, col] != GroveMapTile.Void) return (col, row);
        }
        throw new Exception();
    }

    public (int x, int y) FindFirstNonVoidTileFromRight(int row)
    {
        for (int col = _width - 1; col >= 0; col--)
        {
            if (Map[row, col] != GroveMapTile.Void) return (col, row);
        }
        throw new Exception();
    }


    private List<Instruction> ParseDirectionInput(string input)
    {
        var instructions = new List<Instruction>();

        var turn = 'R'; // first turn
        var index = 0;

        while (true) {
            var nextLeft = input.IndexOf('L', index);
            var nextRight = input.IndexOf('R', index);
            if (nextLeft == -1 && nextRight == -1)
            {
                var lastSteps = int.Parse(input[index..]);
                instructions.Add(new Instruction(turn, lastSteps));
                break;
            }
            var indexOfNextTurn = nextLeft == -1 || nextRight == -1 
                ? Math.Max(input.IndexOf('R', index), input.IndexOf('L', index))
                : Math.Min(input.IndexOf('R', index), input.IndexOf('L', index));

            var steps = int.Parse(input[index..indexOfNextTurn]);
            instructions.Add(new Instruction(turn, steps));

            turn = input[indexOfNextTurn];
            index = indexOfNextTurn + 1;
        }

        return instructions;
    }

    private void ParseMapInput(string input)
    {
        var inputLines = input.Split(Environment.NewLine);
        _height = inputLines.Length;
        _width = inputLines[0].Length;

        Map = new GroveMapTile[_height, _width];
        for (int row = 0; row < _height; row++)
        {
            for (int col = 0; col < _width; col++)
            {
                Map[row, col] = col < inputLines[row].Length
                    ? (GroveMapTile)inputLines[row][col]
                    : GroveMapTile.Void;
            }
        }
    }
}

public sealed class Instruction
{
    public Turn Turn { get; set; }
    public int Steps { get; set; }
    public Instruction(char turn, int steps)
    {
        Turn= (Turn)turn;
        Steps= steps;
    }
}


public enum GroveMapTile
{
    Open = '.',
    Wall = '#',
    Void = ' '
}

public enum Turn
{
    Left = 'L',
    Right = 'R'
}