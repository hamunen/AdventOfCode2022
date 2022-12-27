
namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day21 : BaseDay
{
    private readonly string _input;

    public Day21()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var math = new MonkeyMath(_input);
        var result = math.SolvePart1();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var math = new MonkeyMath(_input);
        var result = math.SolveForHumn();

        return new(result.ToString());
    }
}

public sealed class MonkeyMath
{
    public MonkeyOp[] Operations { get; set; }


    public MonkeyMath(string input) {
        Operations = input.Split(Environment.NewLine).Select(l => MonkeyOp.ParseInputLine(l)).ToArray();
    }

    public long SolvePart1()
    {
        var root = Operations.First(o => o.Name == "root");
        while (!root.IsSolved)
        {
            var unSolvedOps = Operations.Where(o => !o.IsSolved);
            var solvedOps = Operations.Where(o => o.IsSolved);

            var canBeSolved = unSolvedOps
                .Where(unSolved =>
                    solvedOps.Any(op => op.Name == unSolved.Operand1) &&
                    solvedOps.Any(op => op.Name == unSolved.Operand2)
                    );

            if (canBeSolved.Count() == 0) throw new Exception();

            foreach (var op in canBeSolved)
            {
                var op1 = solvedOps.First(solved => solved.Name == op.Operand1);
                var op2 = solvedOps.First(solved => solved.Name == op.Operand2);

                op.Solve(op1, op2);
            }
        }

        return root.Result;
    }

    public void SolveAllNotDependingOnHumn()
    {
        while (true)
        {
            var unSolvedOps = Operations.Where(o => !o.IsSolved && o.Name != "humn" && o.Name != "root");
            var solvedOps = Operations.Where(o => o.IsSolved);

            var canBeSolved = unSolvedOps
                .Where(unSolved =>
                    solvedOps.Any(op => op.Name == unSolved.Operand1) &&
                    solvedOps.Any(op => op.Name == unSolved.Operand2)
                    );

            if (canBeSolved.Count() == 0) return;

            foreach (var op in canBeSolved)
            {
                var op1 = solvedOps.First(solved => solved.Name == op.Operand1);
                var op2 = solvedOps.First(solved => solved.Name == op.Operand2);

                op.Solve(op1, op2);
            }
        }
    }


    // Assuming quite a lot here, that not multiple equations are dependent on humn, and everything else can be solved without it. Turns out that is the case,
    // So we can basically solve the right side of all equations coming down from the root to the humn equation
    public long SolveForHumn()
    {
        var root = Operations.First(o => o.Name == "root");
        var humn = Operations.First(o => o.Name == "humn");
        humn.IsSolved = false;
        SolveAllNotDependingOnHumn();

        var rootOp1 = Operations.First(o => o.Name == root.Operand1);
        var rootOp2 = Operations.First(o => o.Name == root.Operand2);

        // put the unsolved variable to the left, and solved to the right. E.g. pppw = 5
        var eqLeftSideVar = rootOp1.IsSolved ? rootOp2 : rootOp1;
        var eqRightSideResult = rootOp1.IsSolved ? rootOp1.Result : rootOp2.Result;

        Console.WriteLine($"{eqLeftSideVar.Name} = {eqRightSideResult}");

        while (eqLeftSideVar.Name != "humn")
        {

            rootOp1 = Operations.First(o => o.Name == eqLeftSideVar.Operand1);
            rootOp2 = Operations.First(o => o.Name == eqLeftSideVar.Operand2);
            var op = eqLeftSideVar.Operation;
            Console.WriteLine($"{rootOp1.Name} {op} {rootOp2.Name} = {eqRightSideResult}");

            var str = rootOp1.IsSolved ? $"{rootOp1.Result} {op} {rootOp2.Name}" : $"{rootOp1.Name} {op} {rootOp2.Result}";
            Console.WriteLine(str + $" = {eqRightSideResult}");

            // cczh - lfqf = 5
            if (op == '/')
                eqRightSideResult = rootOp1.IsSolved ? rootOp1.Result / eqRightSideResult : rootOp2.Result * eqRightSideResult;
            else if (op == '*')
                eqRightSideResult = rootOp1.IsSolved ? eqRightSideResult / rootOp1.Result : eqRightSideResult / rootOp2.Result;
            else if (op == '+')
                eqRightSideResult = rootOp1.IsSolved ? eqRightSideResult - rootOp1.Result : eqRightSideResult - rootOp2.Result;
            else if (op == '-')
                eqRightSideResult = rootOp1.IsSolved ? rootOp1.Result - eqRightSideResult : rootOp2.Result + eqRightSideResult;

            eqLeftSideVar = rootOp1.IsSolved ? rootOp2 : rootOp1;
            Console.WriteLine($"{eqLeftSideVar.Name} = {eqRightSideResult}");

        }

        return eqRightSideResult;

    }



}


public sealed class MonkeyOp
{
    public string Name { get; set; }
    public bool IsSolved { get; set; }
    public long Result { get; set; }
    public string Operand1 { get; set; }
    public string Operand2 { get; set; }
    public char Operation { get; set; }

    public MonkeyOp(string name, string operand1, char operation, string operand2)
    {
        Name = name;
        IsSolved = false;
        Operand1 = operand1;
        Operand2 = operand2;
        Operation = operation;
    }

    public MonkeyOp(string name, long result)
    {
        Name = name;
        IsSolved = true;
        Result = result;
    }

    public void Solve(MonkeyOp op1, MonkeyOp op2)
    {
        if (!op1.IsSolved || !op2.IsSolved) throw new Exception();
        if (IsSolved) return;

        if (Operation == '+') Result = op1.Result + op2.Result;
        else if (Operation == '-') Result = op1.Result - op2.Result;
        else if (Operation == '*') Result = op1.Result * op2.Result;
        else Result = op1.Result / op2.Result;

        IsSolved = true;
    }

    public static MonkeyOp ParseInputLine(string input)
    {
        var parts1 = input.Split(": ");
        var name = parts1[0];

        var opParts = parts1[1].Trim().Split(' ');
        if (opParts.Length == 1) {
            var result = long.Parse(opParts[0]);
            return new MonkeyOp(name, result);
        }

        var operand1 = opParts[0].Trim();
        var operation = opParts[1].Trim()[0];
        var operand2 = opParts[2].Trim();
        return new MonkeyOp(name, operand1, operation, operand2);
    }

    public override string ToString()
    {
        var str = "";
        if (Operand1 is not null) str += $"{Name} = {Operand1} {Operation} {Operand2}";
        if (IsSolved) str += " = " + Result.ToString();

        return str;
    } 
}