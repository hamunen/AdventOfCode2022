using Microsoft.Win32;

namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day10 : BaseDay
{
    private readonly string _input;
    public Day10()
    {
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
        var device = new CommunicationDevice();
        device.ProcessFullInstructionsInput(_input);
        device.DrawToScreen();

        // Needs to be read from screen :)
        var result = "RLEZFLGE";
        return new(result.ToString());
    }
}


public sealed class CommunicationDevice
{
    private CommunicationDeviceCPU cpu;
    private CommunicationDeviceCRT crt;

    public CommunicationDevice()
    {
        cpu = new CommunicationDeviceCPU();
        crt = new CommunicationDeviceCRT();
    }

    public void ProcessFullInstructionsInput(string input)
    {
        cpu.ProcessFullInstructionsInput(input);
    }

    public void DrawToScreen()
    {
        for (int i = 1; i < cpu.RegisterXHistoryAfterCycle.Count; i++)
        {
            crt.DrawPixel(i, cpu.RegisterXHistoryAfterCycle[i - 1]);
        }
        crt.PrintScreen();
    }
}

public sealed class CommunicationDeviceCRT
{
    public char[,] Screen { get; set; }

    private const int SCREEN_WIDTH = 40;
    private const int SCREEN_HEIGHT = 6;

    public CommunicationDeviceCRT() {
        Screen = new char[SCREEN_HEIGHT, SCREEN_WIDTH];
    }

    public void DrawPixel(int cycle, int spriteMidPosition)
    {
        var row = (cycle - 1) / SCREEN_WIDTH;
        var pixelPosition = (cycle - 1) % SCREEN_WIDTH;

        var pixelIsLit = Math.Abs(pixelPosition - spriteMidPosition) <= 1;

        var pixel = pixelIsLit ? '#' : '.';
        Screen[row, pixelPosition] = pixel;
    }

    public void PrintScreen()
    {
        Console.Clear();
        for (int i = 0; i < SCREEN_HEIGHT; i++)
        {
            var row = "";
            for (int j = 0; j < SCREEN_WIDTH; j++)
            {
                row += Screen[i, j];
            }
            Console.WriteLine(row);
        }
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
