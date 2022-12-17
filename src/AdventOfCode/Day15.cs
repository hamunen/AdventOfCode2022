using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day15 : BaseDay
{
    private readonly string _input;

    public Day15()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var map = new BeaconMap(_input);

        var result = map.GetCountOfPositionsThatCantContainBeacons(2000000);
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var map = new BeaconMap(_input);
        //map.BruteForceFind();

        var result = map.FindDistressBeacon();
        return new(result.ToString());
    }
}

public sealed class BeaconMap
{
    public Dictionary<(int X, int Y), Node> Map { get; set; }
    public List<Sensor> Sensors { get; set; }

    public BeaconMap(string input)
    {
        Map = new();
        Sensors = new();

        ParseInput(input);
    }

    public void ParseInput(string input)
    {
        var lines = input.Split(Environment.NewLine);
        Array.ForEach(lines, l => ParseInputLine(l));
    }

    public void ParseInputLine(string input)
    {
        Match match = Regex.Match(input, @"Sensor at x=(-?\d+), y=(-?\d+): closest beacon is at x=(-?\d+), y=(-?\d+)");
        if (match.Success)
        {
            var sensorX = int.Parse(match.Groups[1].Value);
            var sensorY = int.Parse(match.Groups[2].Value);
            var beaconX = int.Parse(match.Groups[3].Value);
            var beaconY = int.Parse(match.Groups[4].Value);

            Map[(sensorX, sensorY)] = Node.Sensor;
            Map[(beaconX, beaconY)] = Node.Beacon;
            Sensors.Add(new Sensor(sensorX, sensorY, beaconX, beaconY));
        } 
    }
    public int GetCountOfPositionsThatCantContainBeacons(int y)
    {
        var blockedXCoordinatesOnRow = new HashSet<int>();
        Sensors.ForEach(sensor =>
        {
            var yDistance = Math.Abs(sensor.Y - y);
            if (yDistance > sensor.DistanceToClosestBeacon) return;

            var xMaxDistanceOnRow = sensor.DistanceToClosestBeacon - yDistance;
            for (int x = sensor.X - xMaxDistanceOnRow; x <= sensor.X + xMaxDistanceOnRow; x++)
            {
                if (Map.ContainsKey((x, y))) continue;
                blockedXCoordinatesOnRow.Add(x);
            }

        });

        return blockedXCoordinatesOnRow.Count;
    }

    public ulong FindDistressBeacon()
    {
        foreach (var sensor in Sensors)
        {
            // Find points where two sensors' perimeters intersect. If there is only one noncovered position,
            // it must be somewhere where it's on the perimeter of two or more sensors (except on the edge maybe?),
            // otherwise there would be multiple noncovered positions
            foreach (var other in Sensors.Where(o => o.X != sensor.X || o.Y != sensor.Y))
            {
                var intersectionPoints = sensor.FindPerimeterIntersectionPoints(other);
                var foundDistressBeacon = intersectionPoints.FirstOrDefault(p => !Sensors.Any(s => s.IsInSensorCoverageArea(p.x, p.y)));
                if (foundDistressBeacon != (0, 0))
                {
                    // var check = Sensors.Select(s => s.IsInSensorCoverageArea(foundDistressBeacon.x, foundDistressBeacon.y));
                    return (ulong)foundDistressBeacon.x * 4000000UL + (ulong)foundDistressBeacon.y;
                }
            }
        }

        // not found :/
        return 0;
    }


    // not used (anymore ;)
    public void BruteForceFind()
    {
        QuadrantSearch(0, 0, Sensor.MAX_COORD, Sensor.MAX_COORD);
    }

    public void QuadrantSearch(int x, int y, int xLength, int yLength)
    {
        Console.Write($"Quadrant search x{x}-{x+xLength}, y{y}-{y+yLength}...");

        // length 3 7 etc......
        if (xLength == 1 && yLength == 1 && Sensors.Any(s => s.IsInSensorCoverageArea(x, y))) {
            Console.WriteLine(" point is covered");
            return;
        }

        else if (Sensors.Any(s => s.SquareIsFullyCoveredBySensor(x, y, xLength, yLength)))
        {
            // cant be this quarter
            Console.WriteLine(" fully covered");
            return;
        }

        if (xLength == 1 && yLength == 1)
        {
            Console.WriteLine(" wow found it!");
        }

        // 2 lengths in case length is odd
        var newXLength1 = xLength / 2;
        var newXLength2 = xLength - newXLength1;
        var newYLength1 = yLength / 2;
        var newYLength2 = yLength - newYLength1;


        Console.WriteLine(" splitting");
        QuadrantSearch(x, y, newXLength1, newYLength1);
        QuadrantSearch(x + newXLength1, y, newXLength2, newYLength1);

        QuadrantSearch(x, y + newYLength1, newXLength1, newYLength2);
        QuadrantSearch(x + newXLength1, y + newYLength1, newXLength2, newYLength2);

    }
}

public class Sensor
{
    public const int MAX_COORD = 4000000;

    public int X { get; set; }
    public int Y { get; set; }
    public int DistanceToClosestBeacon { get; set; }
    public int Perimeter => DistanceToClosestBeacon + 1;

    public Sensor(int x, int y, int beaconX, int beaconY)
    {
        X = x;
        Y = y;
        DistanceToClosestBeacon = Math.Abs(beaconX - x) + Math.Abs(beaconY - y);
    }

    public bool SquareIsFullyCoveredBySensor(int x, int y, int xLength, int yLength)
    {
        return IsInSensorCoverageArea(x, y) && IsInSensorCoverageArea(x, y + yLength - 1)
            && IsInSensorCoverageArea(x + xLength - 1, y) && IsInSensorCoverageArea(x + xLength - 1, y + yLength - 1);
    }

    public IEnumerable<(int x, int y)> FindPerimeterIntersectionPoints(Sensor other)
    {
        var intersectionPoints = new List<(int x, int y)>();

        if (!OverlapsOnPerimeterWith(other)) return intersectionPoints;

        // 4 lines but only two points we need
        var thisSouthVertex = (x: X, y: Y - Perimeter);
        var thisNorthwardEdge1 = new Line(thisSouthVertex.x, thisSouthVertex.y, 1);
        var thisSouthwardEdge1 = new Line(thisSouthVertex.x, thisSouthVertex.y, -1);

        var thisNorthVertex = (x: X, y: Y + Perimeter);
        var thisNorthwardEdge2 = new Line(thisNorthVertex.x, thisNorthVertex.y, 1);
        var thisSouthwardEdge2 = new Line(thisNorthVertex.x, thisNorthVertex.y, -1);


        var otherSouthVertex = (x: other.X, y: other.Y - other.Perimeter); 
        var otherNorthwardEdge1 = new Line(otherSouthVertex.x, otherSouthVertex.y, 1);
        var otherSouthwardEdge1 = new Line(otherSouthVertex.x, otherSouthVertex.y, -1);

        var otherNorthVertex = (x: other.X, y: other.Y + other.Perimeter);
        var otherNorthwardEdge2 = new Line(otherNorthVertex.x, otherNorthVertex.y, 1);
        var otherSouthwardEdge2 = new Line(otherNorthVertex.x, otherNorthVertex.y, -1);

        // intersections?
        intersectionPoints.AddRange(thisNorthwardEdge1.GetIntersectionPoint(otherSouthwardEdge1));
        intersectionPoints.AddRange(thisNorthwardEdge1.GetIntersectionPoint(otherSouthwardEdge2));
        intersectionPoints.AddRange(thisNorthwardEdge2.GetIntersectionPoint(otherSouthwardEdge1));
        intersectionPoints.AddRange(thisNorthwardEdge2.GetIntersectionPoint(otherSouthwardEdge2));

        intersectionPoints.AddRange(thisSouthwardEdge1.GetIntersectionPoint(otherNorthwardEdge1));
        intersectionPoints.AddRange(thisSouthwardEdge1.GetIntersectionPoint(otherNorthwardEdge2));
        intersectionPoints.AddRange(thisSouthwardEdge2.GetIntersectionPoint(otherNorthwardEdge1));
        intersectionPoints.AddRange(thisSouthwardEdge2.GetIntersectionPoint(otherNorthwardEdge2));

        // add intersection with plane edges?
        var NWToSouth = new Line(0, 0, 0);

        // intersection point needs to be on perimeter..
        return intersectionPoints
            .Where(p => p.x >= 0 && p.x <= MAX_COORD && p.y >= 0 && p.y <= MAX_COORD)
            .Where(p => IsOnPerimeter(p.x, p.y));
        // same direction lines dont intersect --- except they could touch!!?
        // but if they touch..there at elast probably is another intersection...

    }

    public IEnumerable<(int x, int y)> FindRaneIntersectionPoints(Sensor other)
    {
        var intersectionPoints = new List<(int x, int y)>();

        if (!OverlapsOnPerimeterWith(other)) return intersectionPoints;

        // 4 lines but only two points we need
        var thisSouthVertex = (x: X, y: Y - DistanceToClosestBeacon);
        var thisNorthwardEdge1 = new Line(thisSouthVertex.x, thisSouthVertex.y, 1);
        var thisSouthwardEdge1 = new Line(thisSouthVertex.x, thisSouthVertex.y, -1);

        var thisNorthVertex = (x: X, y: Y + DistanceToClosestBeacon);
        var thisNorthwardEdge2 = new Line(thisNorthVertex.x, thisNorthVertex.y, 1);
        var thisSouthwardEdge2 = new Line(thisNorthVertex.x, thisNorthVertex.y, -1);


        var otherSouthVertex = (x: other.X, y: other.Y - other.DistanceToClosestBeacon);
        var otherNorthwardEdge1 = new Line(otherSouthVertex.x, otherSouthVertex.y, 1);
        var otherSouthwardEdge1 = new Line(otherSouthVertex.x, otherSouthVertex.y, -1);

        var otherNorthVertex = (x: other.X, y: other.Y + other.DistanceToClosestBeacon);
        var otherNorthwardEdge2 = new Line(otherNorthVertex.x, otherNorthVertex.y, 1);
        var otherSouthwardEdge2 = new Line(otherNorthVertex.x, otherNorthVertex.y, -1);

        // intersections?
        intersectionPoints.AddRange(thisNorthwardEdge1.GetIntersectionPoint(otherSouthwardEdge1));
        intersectionPoints.AddRange(thisNorthwardEdge1.GetIntersectionPoint(otherSouthwardEdge2));
        intersectionPoints.AddRange(thisNorthwardEdge2.GetIntersectionPoint(otherSouthwardEdge1));
        intersectionPoints.AddRange(thisNorthwardEdge2.GetIntersectionPoint(otherSouthwardEdge2));

        intersectionPoints.AddRange(thisSouthwardEdge1.GetIntersectionPoint(otherNorthwardEdge1));
        intersectionPoints.AddRange(thisSouthwardEdge1.GetIntersectionPoint(otherNorthwardEdge2));
        intersectionPoints.AddRange(thisSouthwardEdge2.GetIntersectionPoint(otherNorthwardEdge1));
        intersectionPoints.AddRange(thisSouthwardEdge2.GetIntersectionPoint(otherNorthwardEdge2));

        // add intersection with plane edges?
        var NWToSouth = new Line(0, 0, 0);

        // intersection point needs to be on perimeter..
        return intersectionPoints
            .Where(p => p.x >= 0 && p.x <= MAX_COORD && p.y >= 0 && p.y <= MAX_COORD)
            .Where(p => IsOnPerimeter(p.x, p.y));
        // same direction lines dont intersect --- except they could touch!!?
        // but if they touch..there at elast probably is another intersection...

        // also remember to check for edge

    }

    public bool OverlapsOnPerimeterWith(Sensor other) 
        => DistanceTo(other) <= Perimeter + other.Perimeter;
    public bool IsOnPerimeter(int x, int y) => DistanceTo(x, y) == Perimeter;
    public int DistanceTo(Sensor other) => DistanceTo(other.X, other.Y);
    public int DistanceTo(int x, int y) => Math.Abs(X - x) + Math.Abs(Y - y);

    public bool IsInSensorCoverageArea(int x, int y)
    {
        // or equal? there is not any case where two beacons are the same distance..
        return DistanceTo(x, y) <= DistanceToClosestBeacon;
    }
}

public sealed class Line
{
    public int A { get; set; } // 1 or -1
    public int B { get; set; }
    public Line(int x, int y, int slope)
    {
        A = slope;
        // y = ax + b
        // b = y - ax
        B = y - slope * x;
    }
    public List<(int x, int y)> GetIntersectionPoint(Line other)
    {
        var list = new List<(int x, int y)>();

        var intersectX = (other.B - B) / (A - other.A);
        var intersectY = A * intersectX + B;

        list.Add((intersectX, intersectY));

        // adding points next to this just in case...
        list.Add(((intersectX + 1), (intersectY - 1)));
        list.Add(((intersectX - 1), (intersectY - 1)));
        list.Add(((intersectX - 1), (intersectY + 1)));
        list.Add(((intersectX + 1), (intersectY + 1)));

        return list;
    }
}

public enum Node
{
    Beacon = 'B',
    Sensor = 'S',
    Empty = '.',
    Covered = '#'
}
