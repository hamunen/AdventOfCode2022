namespace AdventOfCode;

[ExcludeFromCodeCoverage]
public class Day06 : BaseDay
{
    private string _input;
    public Day06()
    {
        _input = File.ReadAllText(InputFilePath);
    }

    public override ValueTask<string> Solve_1()
    {
        var dataStream = new ElfDataStream(_input);
        var result = dataStream.GetStartOfPacketMarkerPosition();
        return new(result.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var dataStream = new ElfDataStream(_input);
        var result = dataStream.GetStartOfMessageMarkerPosition();
        return new(result.ToString());
    }
}

public class ElfDataStream
{
    private const int START_OF_PACKET_SEQUENCE_LENGTH = 4;
    private const int START_OF_MESSAGE_SEQUENCE_LENGTH = 14;

    public string Buffer { get; }
    public ElfDataStream(string buffer)
    {
        Buffer = buffer;
    }

    public int GetStartOfPacketMarkerPosition() 
        => GetUniqueCharacterSequenceEndPositionFromBuffer(
            START_OF_PACKET_SEQUENCE_LENGTH);

    public int GetStartOfMessageMarkerPosition() 
        => GetUniqueCharacterSequenceEndPositionFromBuffer(
            START_OF_MESSAGE_SEQUENCE_LENGTH);

    private int GetUniqueCharacterSequenceEndPositionFromBuffer(
        int sequenceLength)
    {
        var endIndex = sequenceLength - 1;
        int startIndex;
        string currentSequence;
        while (endIndex < Buffer.Length)
        {
            startIndex = endIndex - sequenceLength + 1;
            currentSequence = Buffer.Substring(startIndex, sequenceLength);

            if (CharactersAreUnique(currentSequence, sequenceLength)) break;
            endIndex++;
        }

        return endIndex + 1;
    }

    private static Boolean CharactersAreUnique(string sequence, int sequenceLength)
    {
        return sequence.Distinct().Count() == sequenceLength;
    }
}