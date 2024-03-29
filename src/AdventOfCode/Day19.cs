﻿using System.Collections;
using System.Text.RegularExpressions;

namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day19 : BaseDay
{
    private readonly string _input;

    public Day19()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var situation = new RobotSituation(_input);
        var result = situation.Test();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var situation = new RobotSituation(_input);
        var result = situation.Test2();
        return new(result.ToString());
    }
}

public sealed class RobotSituation
{
    public Blueprint[] Blueprints { get; }
    public RobotSituation(string input) {
        Blueprints = Blueprint.ParseFromInput(input);
    }

    public int Test()
    {
        var qualityLevelSums = 0;
        foreach (var blueprint in Blueprints)
        {
            var scenario = new RobotBuildingScenario(blueprint, 24);
            var scenarioBestValue = scenario.GetHowManyGeodesCanBeGotten(new Dictionary<int, byte>(), 0);
            qualityLevelSums += blueprint.Index * scenarioBestValue;
            Console.WriteLine($"Blueprint {blueprint.Index}, best value {scenarioBestValue}");
        }

        Console.ReadKey();
        return qualityLevelSums;
    }

    public int Test2()
    {
        var value = 0;
        foreach (var blueprint in Blueprints.Where(bp => bp.Index <= 3))
        {
            var scenario = new RobotBuildingScenario(blueprint, 32);
            var scenarioBestValue = scenario.GetHowManyGeodesCanBeGotten(new Dictionary<int, byte>(), 0);
            value *= scenarioBestValue;
            Console.WriteLine($"Blueprint {blueprint.Index}, best value {scenarioBestValue}");
        }

        Console.ReadKey();
        return value;
    }
}

public sealed class RobotBuildingScenario
{
    public Blueprint Blueprint { get; set; }
    private int Ores { get; set; } = 0;
    private int Clays { get; set; } = 0;
    private int Obsidians { get; set; } = 0;
    private int Geodes { get; set; } = 0;
    private int OreRobots { get; set; } = 1;
    private int ClayRobots { get; set; } = 0;
    private int ObsidianRobots { get; set; } = 0;
    private int GeodeRobots { get; set; } = 0;
    private int TimeLeft { get; set; }


    public RobotBuildingScenario(Blueprint blueprint, int timeLeft)
    {
        Blueprint = blueprint;
        TimeLeft = timeLeft;
    }

    public RobotBuildingScenario(Blueprint blueprint, int timeLeft,
        int ores, int clays, int obsidians,
        int oreRobots, int clayRobots, int obsidianRobots, int geodeRobots
        )
    {
        Blueprint = blueprint;
        TimeLeft = timeLeft;
        Ores = ores;
        Clays = clays;
        Obsidians = obsidians;
        OreRobots = oreRobots;
        ClayRobots = clayRobots;
        ObsidianRobots = obsidianRobots;
        GeodeRobots = geodeRobots;
    }

    public static int TheoreticalGeodeCount(int timeLeft)
    {
        var geodes = 0;
        for (int i = timeLeft; i > 0; i--)
        {
            // we could create time-1 geodes after each geodeRobot creation?
            geodes += i - 1;
        }

        return geodes;
    }

    // take out geodes from props too?
    public byte GetHowManyGeodesCanBeGotten(Dictionary<int,byte> bestValues, int bestValueSofarInThisLevel)
    {
        if (TimeLeft == 1) return (byte)GeodeRobots;

        // max amount of geodes we could produce... (if we got .. idk) maybe not working props
        var maxLimit = GeodeRobots * TimeLeft + TheoreticalGeodeCount(TimeLeft);
        if (bestValueSofarInThisLevel > maxLimit) return 0;

        // can do 1 of 5 things: buy each robot or buy nothing..
        var subProblems = new List<RobotBuildingScenario> ();
        if (Ores >= Blueprint.GeodeRobotCostInOre && Obsidians >= Blueprint.GeodeRobotCostInObsidian) subProblems.Add(CreateGeodeRobot());
        if (Ores >= Blueprint.ObsidianRobotCostInOre && Clays >= Blueprint.ObsidianRobotCostInClay) subProblems.Add(CreateObsidianRobot());
        if (Ores >= Blueprint.ClayRobotCostInOre) subProblems.Add(CreateClayRobot());
        if (Ores >= Blueprint.OreRobotCostInOre) subProblems.Add(CreateOreRobot());
        subProblems.Add(CreateNothing());

        byte bestValue = 0;
        foreach (var p in subProblems)
        {
            byte subProblemBestValue;
            //var subProblemHash = p.GetHashCode();
            var subProblemHash = p.GetHashCodeWoGeodes();
            if (bestValues.ContainsKey(subProblemHash))
            {
                subProblemBestValue = bestValues[subProblemHash]; // is this ok?
            } else
            {
                subProblemBestValue = p.GetHowManyGeodesCanBeGotten(bestValues, bestValue);
                bestValues.Add(subProblemHash, subProblemBestValue);
            }
            if (bestValue < subProblemBestValue) bestValue = subProblemBestValue;
        }

        // return bestValue;
        return (byte)(bestValue + GeodeRobots);
    }

    private RobotBuildingScenario CreateNothing()
    {
        return new RobotBuildingScenario(Blueprint, TimeLeft - 1,
            Ores + OreRobots, Clays + ClayRobots, Obsidians + ObsidianRobots,
            OreRobots, ClayRobots, ObsidianRobots, GeodeRobots);
    }

    private RobotBuildingScenario CreateOreRobot()
    {
        var newOres = Ores - Blueprint.OreRobotCostInOre;
        return new RobotBuildingScenario(Blueprint, TimeLeft - 1,
            newOres + OreRobots, Clays + ClayRobots, Obsidians + ObsidianRobots,
            OreRobots + 1, ClayRobots, ObsidianRobots, GeodeRobots);
    }

    private RobotBuildingScenario CreateClayRobot()
    {
        var newOres = Ores - Blueprint.ClayRobotCostInOre;
        return new RobotBuildingScenario(Blueprint, TimeLeft - 1,
            newOres + OreRobots, Clays + ClayRobots, Obsidians + ObsidianRobots,
            OreRobots, ClayRobots + 1, ObsidianRobots, GeodeRobots);
    }

    private RobotBuildingScenario CreateObsidianRobot()
    {
        var newOres = Ores - Blueprint.ObsidianRobotCostInOre;
        var newClays = Clays - Blueprint.ObsidianRobotCostInClay;
        return new RobotBuildingScenario(Blueprint, TimeLeft - 1,
            newOres + OreRobots, newClays + ClayRobots, Obsidians + ObsidianRobots,
            OreRobots, ClayRobots, ObsidianRobots + 1, GeodeRobots);
    }

    private RobotBuildingScenario CreateGeodeRobot()
    {
        var newOres = Ores - Blueprint.GeodeRobotCostInOre;
        var newObsidians = Obsidians - Blueprint.GeodeRobotCostInObsidian;
        return new RobotBuildingScenario(Blueprint, TimeLeft - 1,
            newOres + OreRobots, Clays + ClayRobots, newObsidians + ObsidianRobots,
            OreRobots, ClayRobots, ObsidianRobots, GeodeRobots + 1);
    }

    public override int GetHashCode()
    {
        var hash = Ores;
        hash += Clays * 100;
        hash += Obsidians * 1000;
        hash += Geodes * 10099;
        hash += OreRobots * 100000;
        hash += ClayRobots * 1000020;
        hash += ObsidianRobots * 1039000;
        hash += GeodeRobots * 10009000;
        hash = hash * 23 + TimeLeft * 39700;

        return hash;
    }

    public int GetHashCodeWoGeodes()
    {
        var hash = Ores;
        hash += Clays * 100;
        hash += Obsidians * 1000;
        hash += OreRobots * 10099;
        hash += ClayRobots * 100000;
        hash += ObsidianRobots * 1000020;
        hash += GeodeRobots * 1039000;
        hash = hash * 23 + TimeLeft * 39700;

        return hash;
    }

}

public sealed class Blueprint
{
    public int Index { get; set; }
    public int OreRobotCostInOre { get; set; }
    public int ClayRobotCostInOre { get; set; }
    public int ObsidianRobotCostInOre { get; set; }
    public int ObsidianRobotCostInClay { get; set; }

    public int GeodeRobotCostInOre { get; set; }
    public int GeodeRobotCostInObsidian { get; set; }

    public Blueprint() { }

    public static Blueprint[] ParseFromInput(string input) =>
        input.Split(Environment.NewLine).Select(ParseInput).ToArray();

    private static Blueprint ParseInput(string input)
    {
        var regex = new Regex(@"Blueprint (\d+): Each ore robot costs (\d+) ore\. Each clay robot costs (\d+) ore\. Each obsidian robot costs (\d+) ore and (\d+) clay\. Each geode robot costs (\d+) ore and (\d+) obsidian\.");
        var match = regex.Match(input);

        if (!match.Success) { throw new InvalidDataException(); };

        return new Blueprint()
        {
            Index = int.Parse(match.Groups[1].Value),
            OreRobotCostInOre = int.Parse(match.Groups[2].Value),
            ClayRobotCostInOre = int.Parse(match.Groups[3].Value),
            ObsidianRobotCostInOre = int.Parse(match.Groups[4].Value),
            ObsidianRobotCostInClay = int.Parse(match.Groups[5].Value),
            GeodeRobotCostInOre = int.Parse(match.Groups[6].Value),
            GeodeRobotCostInObsidian = int.Parse(match.Groups[7].Value)
        };

    }
}