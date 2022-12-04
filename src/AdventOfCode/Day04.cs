namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day04 : BaseDay
{
    private readonly IEnumerable<SectionAssignmentPair> _sectionAssignmentPairs;

    public Day04()
    {
        var input = File.ReadAllText(InputFilePath);
        var lines = input.Split(Environment.NewLine);
        _sectionAssignmentPairs =
            lines.Select(l => SectionAssignmentPair.ParseSectionsFrom(l));
    }

    public override ValueTask<string> Solve_1() {
        var countOfPairsWhereSectionsFullyOverlap
            = _sectionAssignmentPairs.Count(p => p.OneSectionFullyContainsOther());
        return new (countOfPairsWhereSectionsFullyOverlap.ToString());
    }

    public override ValueTask<string> Solve_2() {
        var countOfOverlappingPairs =
            _sectionAssignmentPairs.Count(p => p.RangesOverlap());
        return new (countOfOverlappingPairs.ToString());
    }
}

public class SectionAssignment
{
    public int MinSection { get; set; }
    public int MaxSection { get; set; }

    public static SectionAssignment ParseSectionFrom(string input)
    {
        var minAndMaxSections = input.Split('-');
        return new SectionAssignment
        {
            MinSection = Int32.Parse(minAndMaxSections[0]),
            MaxSection = Int32.Parse(minAndMaxSections[1])
        };
    }

    public Boolean FullyContains(SectionAssignment other) =>
        MinSection <= other.MinSection && MaxSection >= other.MaxSection;

    public Boolean OverlapsWith(SectionAssignment other) => 
        Math.Max(MinSection, other.MinSection) <= 
        Math.Min(MaxSection, other.MaxSection);
}

public class SectionAssignmentPair
{
    public SectionAssignment FirstSection { get; set; }
    public SectionAssignment SecondSection { get; set; }

    public static SectionAssignmentPair ParseSectionsFrom(string inputLine)
    {
        var inputPair = inputLine.Split(',');
        return new SectionAssignmentPair
        {
            FirstSection = SectionAssignment.ParseSectionFrom(inputPair[0]),
            SecondSection = SectionAssignment.ParseSectionFrom(inputPair[1])
        };
    }

    public Boolean OneSectionFullyContainsOther() => 
        FirstSection.FullyContains(SecondSection) 
        || SecondSection.FullyContains(FirstSection);

    public Boolean RangesOverlap() => FirstSection.OverlapsWith(SecondSection);
}