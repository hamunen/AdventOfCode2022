namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day05 : BaseDay
{
    private string _input;
    public Day05()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var crane = new CrateMover(_input);
        crane.DoAllMoves();
        var result = crane.GetTopCratesFromEachStack();

        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var crane = new CrateMover9001(_input);
        crane.DoAllMoves();
        var result = crane.GetTopCratesFromEachStack();

        return new(result.ToString());
    }

    public abstract class GiantCrateCrane
    {
        public const int STACK_COLUMN_WIDTH = 4;
        public Stack<char>[] Stacks { get; }
        public IEnumerable<CraneMoveOperation> Moves { get; }

        public GiantCrateCrane(string input)
        {
            var splitInput = input.Split(Environment.NewLine + Environment.NewLine);
            Stacks = ParseStacks(splitInput[0]);
            Moves = CraneMoveOperation.ParseMoves(splitInput[1]);
        }

        public abstract void DoMove(CraneMoveOperation move);

        public void DoAllMoves()
        {
            foreach (var move in Moves)
            {
                DoMove(move);
            }
        }
        public string GetTopCratesFromEachStack()
        {
            var topCrates = "";
            foreach (var stack in Stacks)
            {
                topCrates += stack.Peek();
            }

            return topCrates;
        }

        public static Stack<char>[] ParseStacks(string input)
        {
            var linesFromBottom = input.Split(Environment.NewLine).Reverse();
            var stackNumberLine = linesFromBottom.First();
            var stackContentLines = linesFromBottom.Skip(1);

            var stacks = stackNumberLine
                .Split("  ")
                .Select(x => new Stack<char>()).ToArray();

            foreach (var line in stackContentLines)
            {
                for (var col = 1; col < line.Length; col += STACK_COLUMN_WIDTH)
                {
                    var stackIndex = col / STACK_COLUMN_WIDTH;
                    var crate = line[col];
                    if (Char.IsUpper(crate))
                    {
                        stacks[stackIndex].Push(crate);
                    }

                }
            }

            return stacks;
        }
    }

    public class CrateMover : GiantCrateCrane
    {
        public CrateMover(string input) : base(input) {}

        public override void DoMove(CraneMoveOperation move)
        {
            var sourceStack = Stacks[move.SourceStackId];
            var destinationStack = Stacks[move.DestionationStackId];

            for (var i = 0; i < move.CratesToMove; i++)
            {
                var crate = sourceStack.Pop();
                destinationStack.Push(crate);
            }
        }
    }

    public class CrateMover9001 : GiantCrateCrane
    {
        public CrateMover9001(string input) : base(input) { }

        public override void DoMove(CraneMoveOperation move)
        {
            var sourceStack = Stacks[move.SourceStackId];
            var destinationStack = Stacks[move.DestionationStackId];

            var crates = new List<char>();

            for (var i = 0; i < move.CratesToMove; i++)
            {
                crates.Insert(0, sourceStack.Pop());
            }
            crates.ForEach(destinationStack.Push);
        }
    }

    public class CraneMoveOperation
    {
        public int CratesToMove { get; }
        public int DestionationStackId { get; }
        public int SourceStackId { get; }

        public CraneMoveOperation(int move, int from, int to) {
            CratesToMove = move;
            SourceStackId = from - 1;
            DestionationStackId = to - 1;    
        }

        public static IEnumerable<CraneMoveOperation> ParseMoves(string input)
        {
            var lines = input.Split(Environment.NewLine);

            return lines
                .Select(l => l.Split(' '))
                .Select(ParseLineArray);
        }

        public static CraneMoveOperation ParseLineArray(string[] lineArr)
        {
            var move = Int32.Parse(lineArr[1]);
            var from = Int32.Parse(lineArr[3]);
            var to = Int32.Parse(lineArr[5]);

            return new CraneMoveOperation(move, from, to);
        }
    }
}
