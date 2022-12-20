using Spectre.Console;
using System.Text;

namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day17 : BaseDay
{
    private readonly string _input;

    public Day17()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var tetrisCave = new TetrisCave(_input);

        var result = tetrisCave.DropRocks(2022);
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {

         var tetrisCave = new TetrisCave(_input);

        var result = tetrisCave.DropRocks(1000000000000);
        return new(result.ToString());
    }
}

public sealed class TetrisCave
{
    public List<Material[]> Tetris { get; set; }
    public int HighestRock { get; set; }
    private readonly ShapeGenerator _shapeGenerator;
    private readonly SidewaysMoveGenerator _sidewaysMoveGenerator;
    private int _nextMoveIndex = 0;
    private readonly TetrisMove[] _moves = new TetrisMove[2] { TetrisMove.Sideways, TetrisMove.Down };
    private long CutOffHeight;
    private ushort[] _highestRocks;
    private readonly Dictionary<int, (long, long)> _stateHashHistory;

    // idea: store tetris state as int (or seven shorts i guess) with row maxes. yes!

    public TetrisCave(string input) {
        Tetris = new List<Material[]>
        {
            ShapeGenerator.MakeFloor()
        };
        HighestRock = 0;
        CutOffHeight = 0;
        _shapeGenerator = new ShapeGenerator();
        _sidewaysMoveGenerator = new SidewaysMoveGenerator(input);
        _highestRocks = new ushort[7];
        _stateHashHistory = new();
    }

    public long DropRocks(long amountOfRocks)
    {
        for (long i = 0; i < amountOfRocks; i++)
        {
            DropNextShape();

            // patterns repeat - checking against a state hash if current state has already existed
            // if it has, we can use that to "simulate" similar rounds until (almost) the end
            var hash = CreateStateHash();
            if (IsStateFoundInHistory(hash))
            {
                var (cutOffFromHistory, indexOfStateHash) = _stateHashHistory[hash];
                var roundSize = i - indexOfStateHash;
                var roundsToSimulate = (amountOfRocks - i) / roundSize;
                var roundCutoffIncrease = CutOffHeight - cutOffFromHistory;
                CutOffHeight += roundsToSimulate * (roundCutoffIncrease);
                i += roundSize * roundsToSimulate;
                _stateHashHistory.Clear();
            }
            SaveHistory(hash, i);
        }
        return CutOffHeight + HighestRock;
    }

    private bool IsStateFoundInHistory(int stateHash) => _stateHashHistory.ContainsKey(stateHash);

    public void SaveHistory(int stateHash, long rockIndex) => _stateHashHistory.Add(stateHash, (CutOffHeight, rockIndex));

    public int CreateStateHash()
    {
        var hash = _nextMoveIndex * 23;
        hash = (hash * 397) ^ _shapeGenerator.CurrentShapeIndex * 27;
        hash = (hash * 13) ^ _sidewaysMoveGenerator.CurrentMoveIndex * 41;
        for (int i = 0; i < 7; i++)
        {
            hash = (hash * 397) ^ 7 * i * _highestRocks[i];
        }
        return hash;
    }

    public void Draw(bool waitKeyForNext)
    {
        StringBuilder builder = new();
        var reversedTetris = Tetris.ToArray().Reverse();
        foreach (var row in reversedTetris)
        {
            builder.Append('|');
            foreach (var Material in row)
            {
                builder.Append(Material == Material.Air ? '.' : (char)Material);
            }
            builder.AppendLine("|");
        }
        Console.Clear();
        Console.WriteLine(builder.ToString());

        if (waitKeyForNext)
        {
            Console.ReadKey();
        }
    }

    public void DropNextShape()
    {
        var activeShapeLowestRow = HighestRock + 4;
        var activeShapeHeight = AppendNewShape();

        var stoppeMoving = false;
        while (!stoppeMoving)
        {
            //Draw(false);
            var nextMove = _moves[_nextMoveIndex];
            _nextMoveIndex++;
            if (_nextMoveIndex >= _moves.Length) _nextMoveIndex = 0;

            stoppeMoving = !DoMove(nextMove, activeShapeLowestRow, activeShapeHeight);
            if (!stoppeMoving && nextMove == TetrisMove.Down) activeShapeLowestRow--;
        }

        HighestRock = Math.Max(HighestRock, activeShapeLowestRow + activeShapeHeight - 1); // shape can go lower than currently highest!

        //Draw(false);
        
        CutOffBlockedRows();
        SetHighestRockPositions();
    }

    private void CutOffBlockedRows()
    {
        var highestBlockedRow = GetHighestBlockedRow();
        if (highestBlockedRow == 0) return;
        Tetris.RemoveRange(0, highestBlockedRow);
        CutOffHeight += highestBlockedRow;
        HighestRock -= highestBlockedRow;
    }

    private void SetHighestRockPositions()
    {
        for (int col = 0; col < 7; col++)
        {
            for (int row = HighestRock; row > 0; row--)
            {
                if (Tetris[row][col] == Material.Air) continue;
                _highestRocks[col] = (ushort)row;
                break;
            }
        }
    }

    // simpler cutoff - where each col is a rock...
    private int GetHighestBlockedRow()
    {
        for (int row = HighestRock; row >= 0; row--)
        {
            var allRocks = true;
            for (int col = 0; col < 7; col++)
            {
                if (Tetris[row][col] == Material.Air)
                {
                    allRocks = false; break;
                }
            }
            if (allRocks) return row;
        }

        return 0;
    }

    private bool DoMove(TetrisMove move, int activeShapeLowestRow, int activeShapeHeight)
    {
        if (move == TetrisMove.Down)
        {
            if (!CanMoveDown(activeShapeLowestRow, activeShapeHeight))
            {
                TurnActiveShapeToRock(activeShapeLowestRow, activeShapeHeight);
                return false;
            }
            MoveDown(activeShapeLowestRow, activeShapeHeight);
            activeShapeLowestRow--;
        }

        if (move == TetrisMove.Sideways)
        {
            var nextSideWaysMove = _sidewaysMoveGenerator.GenerateNextMove();
            if (!CanMoveSideways(nextSideWaysMove, activeShapeLowestRow, activeShapeHeight)) return true;
            MoveSideWays(nextSideWaysMove, activeShapeLowestRow, activeShapeHeight);
        }

        // returns true if move succesful
        return true;
    }

    private void MoveSideWays(TetrisMove move, int activeShapeLowestRow, int activeShapeHeight)
    {
        if (move == TetrisMove.Left)
        {
            MoveLeft(activeShapeLowestRow, activeShapeHeight);
        } else
        {
            MoveRight(activeShapeLowestRow, activeShapeHeight);
        }
    }

    private void MoveLeft(int activeShapeLowestRow, int activeShapeHeight)
    {
        for (int row = activeShapeLowestRow; row < activeShapeLowestRow + activeShapeHeight; row++)
        {
            for (int col = 1; col < 7; col++)
            {
                if (Tetris[row][col] != Material.MovingRock) continue;
                Tetris[row][col - 1] = Material.MovingRock;
                Tetris[row][col] = Material.Air; // turn current cell back to air
                //Draw(false);
            }
        }
    }

    private void MoveRight(int activeShapeLowestRow, int activeShapeHeight)
    {
        for (int row = activeShapeLowestRow; row < activeShapeLowestRow + activeShapeHeight; row++)
        {
            for (int col = 5; col >= 0; col--)
            {
                if (Tetris[row][col] != Material.MovingRock) continue;
                Tetris[row][col + 1] = Material.MovingRock;
                Tetris[row][col] = Material.Air; // turn current cell back to air
            }
        }
    }

    private bool CanMoveSideways(TetrisMove move, int activeShapeLowestRow, int activeShapeHeight)
        => move == TetrisMove.Left 
        ? CanMoveLeft(activeShapeLowestRow, activeShapeHeight) 
        : CanMoveRight(activeShapeLowestRow, activeShapeHeight);

    private bool CanMoveLeft(int activeShapeLowestRow, int activeShapeHeight)
    {
        for (int row = activeShapeLowestRow; row < activeShapeLowestRow + activeShapeHeight; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                if (Tetris[row][col] != Material.MovingRock) continue;
                if (col == 0) return false; // moving rock next to wall so cant move
                var materialOnLeft = Tetris[row][col-1];
                if (materialOnLeft == Material.Rock) return false;
            }
        }
        return true;
    }

    private bool CanMoveRight(int activeShapeLowestRow, int activeShapeHeight)
    {
        for (int row = activeShapeLowestRow; row < activeShapeLowestRow + activeShapeHeight; row++)
        {
            for (int col = 6; col >= 0; col--)
            {
                if (Tetris[row][col] != Material.MovingRock) continue;
                if (col == 6) return false; // moving rock next to wall so cant move
                var materialOnRight = Tetris[row][col + 1];
                if (materialOnRight == Material.Rock) return false;
            }
        }
        return true;
    }

    private void MoveDown(int activeShapeLowestRow, int activeShapeHeight)
    {
        for (int row = activeShapeLowestRow; row < activeShapeLowestRow + activeShapeHeight; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                if (Tetris[row][col] != Material.MovingRock) continue;
                Tetris[row - 1][col] = Material.MovingRock;
                Tetris[row][col] = Material.Air; // turn current cell back to air
            }
        }
    }

    private void TurnActiveShapeToRock(int activeShapeLowestRow, int activeShapeHeight)
    {
        for (int row = activeShapeLowestRow; row < activeShapeLowestRow + activeShapeHeight; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                if (Tetris[row][col] != Material.MovingRock) continue;
                Tetris[row][col] = Material.Rock;
            }
        }
    }

    private bool CanMoveDown(int activeShapeLowestRow, int activeShapeHeight)
    {
        for (int row = activeShapeLowestRow; row < activeShapeLowestRow + activeShapeHeight; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                if (Tetris[row][col] != Material.MovingRock) continue;
                var materialOneRowDown = Tetris[row - 1][col];
                if (materialOneRowDown == Material.Floor || materialOneRowDown == Material.Rock) return false;
            }
        }
        return true;
    }

    private int AppendNewShape()
    {
        Tetris.RemoveRange(HighestRock+1, Tetris.Count - HighestRock - 1); 

        Tetris.AddRange(ShapeGenerator.MakeEmptyRows());
        var (shape, height) = _shapeGenerator.GenerateNextShape();
        Tetris.AddRange(shape);

        return height;
    }
}

public sealed class ShapeGenerator
{
    public int CurrentShapeIndex { get; private set; }
    public static Material[] MakeFloor() => Enumerable.Repeat(Material.Floor, 7).ToArray();
    public static List<Material[]> MakeEmptyRows() => new() { new Material[7], new Material[7], new Material[7] };


    private readonly ITetrisShape[] _shapeRotation = new ITetrisShape[5]
    {
        new HorizontalLineShape(),
        new PlusShape(),
        new ReverseLShape(),
        new VerticalLineShape(),
        new SquareShape()
    };

    public (Material[][], int) GenerateNextShape()
    {
        var shape = _shapeRotation[CurrentShapeIndex].GetShape().ToArray();
        var height = _shapeRotation[CurrentShapeIndex].GetShapeHeight();
        CurrentShapeIndex++;
        if (CurrentShapeIndex >= _shapeRotation.Length) CurrentShapeIndex = 0;

        return (shape, height);
    }

}

public interface ITetrisShape
{
    public List<Material[]> GetShape();
    public int GetShapeHeight();
}

public sealed class HorizontalLineShape : ITetrisShape
{
    public List<Material[]> GetShape()
    {
        var shape = new List<Material[]>();
        var row1 = new Material[7] { 0, 0, Material.MovingRock, Material.MovingRock, Material.MovingRock, Material.MovingRock, 0 };
        shape.Add(row1);
        return shape;
    }

    public int GetShapeHeight() => 1;
}

public sealed class PlusShape : ITetrisShape
{
    public List<Material[]> GetShape()
    {
        var shape = new List<Material[]>();
        var row1 = new Material[7] { 0, 0, 0, Material.MovingRock, 0, 0, 0 };
        var row2 = new Material[7] { 0, 0, Material.MovingRock, Material.MovingRock, Material.MovingRock, 0, 0 };
        shape.Add(row1);
        shape.Add(row2);
        shape.Add((Material[])row1.Clone());
        return shape;
    }

    public int GetShapeHeight() => 3;
}

public sealed class ReverseLShape : ITetrisShape
{
    public List<Material[]> GetShape()
    {
        var shape = new List<Material[]>();
        var row1 = new Material[7] { 0, 0, Material.MovingRock, Material.MovingRock, Material.MovingRock, 0, 0 };
        var row2 = new Material[7] { 0, 0, 0, 0, Material.MovingRock, 0, 0 };

        shape.Add(row1);
        shape.Add(row2);
        shape.Add((Material[])row2.Clone());
        return shape;
    }

    public int GetShapeHeight() => 3;
}

public sealed class VerticalLineShape : ITetrisShape
{
    public List<Material[]> GetShape()
    {
        var shape = new List<Material[]>();
        var row = new Material[7] { 0, 0, Material.MovingRock, 0, 0, 0, 0 };

        shape.Add((Material[])row.Clone());
        shape.Add((Material[])row.Clone());
        shape.Add((Material[])row.Clone());
        shape.Add((Material[])row.Clone());
        return shape;
    }
    public int GetShapeHeight() => 4;
}

public sealed class SquareShape : ITetrisShape
{
    public List<Material[]> GetShape()
    {
        var shape = new List<Material[]>();
        var row = new Material[7] { 0, 0, Material.MovingRock, Material.MovingRock, 0, 0, 0 };

        shape.Add((Material[])row.Clone());
        shape.Add((Material[])row.Clone());
        return shape;
    }
    public int GetShapeHeight() => 2;
}

public sealed class SidewaysMoveGenerator
{

    private readonly TetrisMove[] _movesRotation;
    public int CurrentMoveIndex { get;  private set; }

    public SidewaysMoveGenerator(string input)
    {
        _movesRotation = input.Select(c => (TetrisMove)c).ToArray();
        CurrentMoveIndex = 0;
    }

    public TetrisMove GenerateNextMove()
    {
        var move = _movesRotation[CurrentMoveIndex];
        CurrentMoveIndex++;
        if (CurrentMoveIndex >= _movesRotation.Length) CurrentMoveIndex = 0;

        return move;
    }

}

public enum Material
{
    Floor = '-',
    Rock = '#',
    MovingRock = '@',
    Air = 0
}

public enum TetrisMove
{
    Sideways = 's',
    Down = 'd',

    Left = '<',
    Right = '>'
}