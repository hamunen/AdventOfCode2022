using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day16 : BaseDay
{
    private readonly string _input;

    public Day16()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var volcano = new Volcano(_input);
        var finder = new Finder(volcano.AllValves);
        var result = finder.FindRoutes();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        return new(2.ToString());
    }
}

public sealed class Volcano
{
    public Valve Start { get; set; }
    public Valve[] AllValves { get; set; }


    public Volcano(string input)
    {
        var lines = input.Split(Environment.NewLine).ToList();
        var valves = lines.Select(line => ParseValve(line)).ToArray();

        lines.ForEach(x => { ParseLeadsTos(x, valves); });
        Start = valves.First(v => v.Name == "AA");
        AllValves = valves;

        CalculateTraversalTimes();
    }

    public Valve ParseValve(string input)
    {
        var name = input[6..8];
        var flowRate = int.Parse(input[23..(input.IndexOf(';'))]);

        return new Valve(name, flowRate);
    }

    private void ParseLeadsTos(string input, Valve[] valves)
    {
        var name = input[6..8];
        var leadsToStrings = input[(input.IndexOf("to valve") + 9)..].Trim()
            .Split(", ");

        var valve = valves.First(v => v.Name == name);
        var toValvesCount = leadsToStrings.Length;

        valve.LeadsTo = new Valve[toValvesCount];
        for (int i = 0; i < toValvesCount; i++)
        {
            valve.LeadsTo[i] = valves.First(v => v.Name == leadsToStrings[i]);
        }
    }

    public void CalculateTraversalTimes()
    {
        foreach (var valve in AllValves)
        {
            foreach (var other in AllValves.Where(v => v.Name != valve.Name))
            {
                valve.TimeToTravelTo[other.Name] = Finder.FindTraversalCost(valve, other, AllValves);
            }
        }
    }
}

// OK I think I could dijkstra this also?
// when valve is turned on, it's basically "visited". yes. yes. yes.
// or maybe no? since these are variable values
public sealed class Finder
{
    public Valve[] AllValves { get; set; }
    public List<Route> FinishedRoutes { get; set; }

    public Finder(Valve[] allValves)
    {
        AllValves = allValves;
        FinishedRoutes = new List<Route>();
    }

    public IEnumerable<Route> Routes { get; set; }  
    // brute force try everything?
    // consider only valves with some value

    public int FindRoutes()
    {
        var start = AllValves.First(v => v.Name == "AA");
        var rootRoute = new Route(start, 30);
        TraverseNext(rootRoute);
        var first = FinishedRoutes
            .OrderByDescending(r => r.ValueAtEnd).First();

        var ordered = FinishedRoutes.OrderByDescending(r => r.ValueAtEnd);

        /*
        Console.WriteLine();
        while (first.Parent != null)
        {
            Console.WriteLine(first.ToString());
            Console.WriteLine(first.DetailsToString());
            first = first.Parent;
        }*/

        return FinishedRoutes
            .Select(r => r.ValueAtEnd)
            .OrderDescending()
            .First();
    }

    public void TraverseNext(Route route)
    {
 
        var current = AllValves.First(v => v.Name == route.CurrentPosition);
        var timeLeft = route.TimeLeft;
        var nextMovesWithAnyValue = AllValves
            .Where(v => !route.OpenedValves.Contains(v.Name))
            .Where(v => v.FlowRate > 0)
            .Where(v => v.GetValueIfTraveledAndOpened(current, timeLeft) > 0);

        if (!nextMovesWithAnyValue.Any())
        {
            // no good moves left
            route.Finished = true;
            FinishedRoutes.Add(route);
            return;
        }

        foreach (var move in nextMovesWithAnyValue)
        {
            var timeToTravelAndOpen = current.TimeToTravelTo[move.Name] + 1;
            var newRoute = new Route(route, move, timeLeft - timeToTravelAndOpen);
            if (newRoute.Finished)
            {
                FinishedRoutes.Add(newRoute);
                continue;
            }
            TraverseNext(newRoute);
        }
    }


    public static int FindTraversalCost(Valve start, Valve destination, Valve[] allValves)
    {
        var unvisited = new Dictionary<Valve, int> ();

        foreach (var valve in allValves)
        {
            unvisited.Add(valve, int.MaxValue);
        }
        unvisited[start] = 0;

        while (unvisited.Count > 0)
        {
            var current = unvisited.OrderBy(u => u.Value).First();
            unvisited.Remove(current.Key);
            
            if (current.Key.Name == destination.Name)
            {
                // found it, do something
                return current.Value;
            }

            var unvisitedNeighbors = unvisited
                .Where(u => current.Key.LeadsTo.Any(l => l.Name == u.Key.Name));
            foreach (var neighbor in unvisitedNeighbors)
            {
                var dist = current.Value + 1;
                if (dist < neighbor.Value)
                {
                    unvisited[neighbor.Key] = dist;
                    neighbor.Key.Parent = current.Key;
                }
            }
        }


        return 0;

    }
}

public class ValveInSearch : Valve
{
    public ValveInSearch(string name, int flowRate) : base(name, flowRate) { }

    public ValveInSearch(Valve valve) : base(valve.Name, valve.FlowRate)
    { 
    }
}

public sealed class Route
{
    public IEnumerable<string> OpenedValves { get; set; }
    public string CurrentPosition { get; set; }
    public int ValueAtEnd { get; set; }
    public int TimeLeft { get; set; }
    public bool Finished { get; set; }
    public Route Parent { get; set; }

    public Route(Route parent, Valve newValve, int timeLeft)
    {
        OpenedValves = parent.OpenedValves.Append(newValve.Name);
        CurrentPosition = newValve.Name;
        TimeLeft = timeLeft;
        // not sure if needs to be minused?
        ValueAtEnd = parent.ValueAtEnd + newValve.FlowRate * (timeLeft);
        Finished = timeLeft <= 2 ? true : false;
        Parent = parent;
    }

    public Route (Valve rootValve, int timeLeft)
    {
        OpenedValves = new List<string>();
        CurrentPosition = rootValve.Name;
        TimeLeft = timeLeft;
        ValueAtEnd = 0;
        Finished = false;
    }

    public override string ToString()
    {
        if (Parent is null) return CurrentPosition;
        return Parent.ToString() + " -> " + CurrentPosition;
    }

    public string DetailsToString()
    {
        var openedValvesStr = "";
        OpenedValves.ToList().ForEach(v => openedValvesStr += " " + v);
        return $"{CurrentPosition}, Opened {openedValvesStr}, value {ValueAtEnd}, time left {TimeLeft}";
    }
}

public class Valve
{
    public string Name { get; set; }
    public int FlowRate { get; set; }
    public Valve[] LeadsTo { get; set; }
    public Dictionary<string, int> TimeToTravelTo { get; set; }

    public Valve Parent { get; set; }

    public int CurrentSearchValue { get; set; }
    public bool IsOpen { get; set; }


    // pathCostTo...
    public int GetValueIfTraveledAndOpened(Valve from, int timeLeft)
    {
        if (FlowRate == 0) return 0;
        var timeAfterOpening = timeLeft - from.TimeToTravelTo[Name] - 1;
        if (timeAfterOpening <= 0) return 0;
        return FlowRate * timeAfterOpening;
    }

    public Dictionary<Valve, int> GetValuesTo(int TimeRemaining)
    {
        var valves = new Dictionary<Valve, int>();

        return valves;
    }

    public Valve(string name, int flowRate)
    {
        Name = name;
        FlowRate = flowRate;
        TimeToTravelTo = new();
    }

    public override string ToString()
    {
        var str = Name + ", leads to";
        foreach (var valve in LeadsTo)
        {
            str += " " + valve.Name;
        }
        return str;
    }



}