namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day08 : BaseDay
{
    private string _input;
    public Day08()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var grid = new TreeGrid(_input);
        var result = grid.CountVisibleTrees();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var grid = new TreeGrid(_input);
        var result = grid.GetHighestScenicScore();
        return new(result.ToString());
    }
}

public class TreeGrid
{
    public int[,] Grid { get; set; }
    private int _bottomEdgeRow;
    private int _rightEdgeColumn;

    public TreeGrid(string input)
    {
        var lines = input.Split(Environment.NewLine);
        _bottomEdgeRow = lines.Length;
        _rightEdgeColumn = lines[0].Length;
        Grid = new int[_bottomEdgeRow, _bottomEdgeRow];

        for ( int row = 0; row < _bottomEdgeRow; row++ )
        {
            for ( int col = 0; col < _rightEdgeColumn; col++ )
            {
                Grid[row, col] = Int32.Parse(lines[row][col].ToString());
            }
        }
    }

    public int GetHighestScenicScore()
    {
        var maxScore = 0;
        for (int row = 0; row < _bottomEdgeRow; row++)
        {
            for (int col = 0; col < _rightEdgeColumn; col++)
            {
                var score = GetScenicScore(row, col);
                if (score > maxScore) { maxScore = score; }
            }
        }

        return maxScore;
    }

    public int GetScenicScore(int row, int col)
    {
        var thisTree = Grid[row, col];

        var treesLookingLeft = GetTreesOnLeft(row, col).Reverse();
        var treesLookingRight = GetTreesOnRight(row, col);
        var treesLookingUp = GetTreesOnTop(row, col).Reverse();
        var treesLookingDown = GetTreesOnBottom(row, col);

        return 
            GetViewingDistance(treesLookingLeft, thisTree)
        * GetViewingDistance(treesLookingRight, thisTree)
        * GetViewingDistance(treesLookingUp, thisTree)
        * GetViewingDistance(treesLookingDown, thisTree);
    }

    private static int GetViewingDistance(IEnumerable<int> trees, int thisTree)
    {
        int distance = 0;
        for (int i = 0; i < trees.Count(); i++)
        {
            distance++;
            if (trees.ElementAt(i) >= thisTree) break;
        }
        return distance;
    }

    public int CountVisibleTrees()
    {
        var visibleTrees = 0;
        for (int row = 0; row < _bottomEdgeRow; row++)
        {
            for (int col = 0; col < _rightEdgeColumn; col++)
            {
                if(IsTreeVisible(row, col)) visibleTrees++;
            }
        }

        return visibleTrees;
    }

    public Boolean IsTreeVisible(int row, int col)
    {
        if (row == 0 || col == 0 ||
            row == _bottomEdgeRow || col == _rightEdgeColumn)
            return true;

        var thisTree = Grid[row, col];

        var visibleFromLeft = GetTreesOnLeft(row, col).All(t => t < thisTree);
        var visibleFromRight = GetTreesOnRight(row, col).All(t => t < thisTree);
        var visibleFromTop = GetTreesOnTop(row, col).All(t => t < thisTree);
        var visibleFromBottom = GetTreesOnBottom(row, col).All(t => t < thisTree);

        return visibleFromLeft || visibleFromRight || visibleFromTop || visibleFromBottom;
    }

    private IEnumerable<int>  GetTreesOnLeft(int row, int col) 
        => GetRowInRange(row, 0, col);
    private IEnumerable<int>  GetTreesOnRight(int row, int col) 
        => GetRowInRange(row, col+1, _rightEdgeColumn);
    private IEnumerable<int> GetTreesOnTop(int row, int col)
        => GetColInRange(col, 0, row);
    private IEnumerable<int> GetTreesOnBottom(int row, int col)
        => GetColInRange(col, row+1, _bottomEdgeRow);

    private IEnumerable<int> GetRowInRange(int row, int startCol, int endCol) 
        => Enumerable.Range(startCol, endCol - startCol)
            .Select(col => Grid[row, col]);

    private IEnumerable<int> GetColInRange(int col, int startRow, int endRow)
        => Enumerable.Range(startRow, endRow - startRow)
            .Select(row => Grid[row, col]);
}