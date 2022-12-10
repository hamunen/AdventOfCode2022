using Microsoft.Win32;

namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day10 : BaseDay
{
    private string _input;
    public Day10()
    {
        Console.WriteLine(InputFilePath);
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var cpu = new CommunicationDeviceCPU();
        cpu.ProcessFullInstructionsInput(_input);

        var result = 0;
        result += cpu.GetSignalStrengthDuringCycle(20);
        result += cpu.GetSignalStrengthDuringCycle(60);
        result += cpu.GetSignalStrengthDuringCycle(100);
        result += cpu.GetSignalStrengthDuringCycle(140);
        result += cpu.GetSignalStrengthDuringCycle(180);
        result += cpu.GetSignalStrengthDuringCycle(220);

        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var result = 2;
        return new(result.ToString());
    }
}

public sealed class CommunicationDeviceCPU
{
    public int CurrentCycle { get; set; }
    public int RegisterX { get; set; }
    public List<int> RegisterXHistoryAfterCycle { get; set; }

    public CommunicationDeviceCPU()
    {
        RegisterX = 1;
        CurrentCycle = 0;
        RegisterXHistoryAfterCycle = new List<int>() { RegisterX };
    }

    public int GetSignalStrengthDuringCycle(int cycle)
    {
        if (cycle == 0) return 0;
        var regXDuringCycle = RegisterXHistoryAfterCycle[cycle - 1];

        return regXDuringCycle * cycle;
    }

    public void ProcessFullInstructionsInput(string input)
    {
        var lines = input.Split(Environment.NewLine);
        foreach (var line in lines) { ExecuteInstruction(line); }
    }

    public void ExecuteInstruction(string input)
    {
        var instructionAndArg = input.Trim().Split(" ");
        var instruction = instructionAndArg[0];

        if (instruction == "addx")
        {
            var argV = Int32.Parse(instructionAndArg[1]);
            ExecuteAddX(argV);
            return;
        }

        ExecuteNoop();
    }

    private void ExecuteAddX(int argV)
    {
        RegisterXHistoryAfterCycle.Add(RegisterX);
        RegisterXHistoryAfterCycle.Add(RegisterX + argV);
        RegisterX += argV;
        CurrentCycle += 2;
    }

    private void ExecuteNoop()
    {
        RegisterXHistoryAfterCycle.Add(RegisterX);
        CurrentCycle++;
    }
}
