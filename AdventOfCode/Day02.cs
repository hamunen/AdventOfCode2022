namespace AdventOfCode;

public class Day02 : BaseDay
{
    private readonly string _input;
    private readonly string[] _roundsInput;

    public Day02()
    {
        _input = File.ReadAllText(InputFilePath);
        _roundsInput = _input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    }


    public override ValueTask<string> Solve_1()
    {
        var roundsBasedOnPlayerActionInput = _roundsInput
            .Select(inputLine => RockPaperScissorsRound.GetRoundFromInputLine_PlayerInputAsShape(inputLine));
        var totalScore = roundsBasedOnPlayerActionInput.Sum(r => r.GetRoundScore());
        return new ValueTask<string>(totalScore.ToString());
    }

    public override ValueTask<string> Solve_2()
    {
        var roundsBasedOnStrategyInput = _roundsInput
            .Select(inputLine => RockPaperScissorsRound.GetRoundFromInputLine_PlayerInputAsStrategy(inputLine));
        var totalScore = roundsBasedOnStrategyInput.Sum(r => r.GetRoundScore());
        return new ValueTask<string>(totalScore.ToString());
    }
}

public class RockPaperScissorsRound
{
    private RockPaperScissorsShape OpponentShape { get; }
    private RockPaperScissorsShape PlayerShape { get; }

    public const int WIN_SCORE = 6;
    public const int DRAW_SCORE = 3;


    public RockPaperScissorsRound(RockPaperScissorsShape opponentShape, RockPaperScissorsShape playerShape)
    {
        PlayerShape = playerShape;
        OpponentShape = opponentShape;
    }

    public RockPaperScissorsRound(RockPaperScissorsShape opponentShape, RockPaperScissorsStrategy playerStrategy)
    {
        OpponentShape = opponentShape;
        PlayerShape = opponentShape.GetPlayerShapeForStrategy(playerStrategy);
    }

    public int GetRoundScore()
    {
        var matchScore = PlayerShape.GetScoreAgainst(OpponentShape);
        var shapeScore = PlayerShape.ShapeScore;
        return matchScore + shapeScore;
    }

    public static RockPaperScissorsRound GetRoundFromInputLine_PlayerInputAsShape(string inputLine)
    {
        var input = inputLine.Split(" ");
        var opponentShapeInput = input[0][0];
        var playerShapeInput = input[1][0];

        return new RockPaperScissorsRound(
            RockPaperScissorsShape.GetShapeFromInput(opponentShapeInput),
            RockPaperScissorsShape.GetShapeFromInput(playerShapeInput));
    }

    public static RockPaperScissorsRound GetRoundFromInputLine_PlayerInputAsStrategy(string inputLine)
    {
        var input = inputLine.Split(" ");
        var opponentShapeInput = input[0][0];
        var playerStrategyInput = input[1][0];

        return new RockPaperScissorsRound(
            RockPaperScissorsShape.GetShapeFromInput(opponentShapeInput),
            (RockPaperScissorsStrategy)playerStrategyInput
            );
    }

}

public enum RockPaperScissorsStrategy
{
    Lose = 'X',
    Draw = 'Y',
    Win = 'Z'
}

public interface RockPaperScissorsShape
{
    // kato tarviiko public?
    public int ShapeScore { get; }

    public static RockPaperScissorsShape GetShapeFromInput(char input)
    {
        switch (input)
        {
            case 'X':
            case 'A':
                return new Rock();
            case 'Y':
            case 'B':
                return new Paper();
            case 'Z':
            case 'C':
                return new Scissors();
            default:
                throw new InvalidDataException($"input had unknown code {input}");
        }
    }

    public int GetScoreAgainst(RockPaperScissorsShape otherShape);
    public RockPaperScissorsShape GetPlayerShapeForStrategy(RockPaperScissorsStrategy playerStrategy);
}

public class Rock : RockPaperScissorsShape
{
    public int ShapeScore => 1;

    public int GetScoreAgainst(RockPaperScissorsShape otherShape)
    {
        if (otherShape is Rock) return RockPaperScissorsRound.DRAW_SCORE;
        if (otherShape is Scissors) return RockPaperScissorsRound.WIN_SCORE;
        return 0;
    }

    public RockPaperScissorsShape GetPlayerShapeForStrategy(RockPaperScissorsStrategy playerStrategy)
    {
        switch (playerStrategy) {
            case RockPaperScissorsStrategy.Draw:
                return new Rock();
            case RockPaperScissorsStrategy.Win:
                return new Paper();
            default:
                return new Scissors();
        }
    }
}

public class Paper : RockPaperScissorsShape
{
    public int ShapeScore => 2;

    public int GetScoreAgainst(RockPaperScissorsShape otherShape)
    {
        if (otherShape is Paper) return RockPaperScissorsRound.DRAW_SCORE;
        if (otherShape is Rock) return RockPaperScissorsRound.WIN_SCORE;
        return 0;
    }

    public RockPaperScissorsShape GetPlayerShapeForStrategy(RockPaperScissorsStrategy playerStrategy)
    {
        switch (playerStrategy)
        {
            case RockPaperScissorsStrategy.Draw:
                return new Paper();
            case RockPaperScissorsStrategy.Win:
                return new Scissors();
            default:
                return new Rock();
        }
    }
}

public class Scissors : RockPaperScissorsShape
{
    public int ShapeScore => 3;

    public int GetScoreAgainst(RockPaperScissorsShape otherShape)
    {
        if (otherShape is Scissors) return RockPaperScissorsRound.DRAW_SCORE;
        if (otherShape is Paper) return RockPaperScissorsRound.WIN_SCORE;
        return 0;
    }

    public RockPaperScissorsShape GetPlayerShapeForStrategy(RockPaperScissorsStrategy playerStrategy)
    {
        switch (playerStrategy)
        {
            case RockPaperScissorsStrategy.Draw:
                return new Scissors();
            case RockPaperScissorsStrategy.Win:
                return new Rock();
            default:
                return new Paper();
        }
    }
}


// X = A = Rock
// Y = B = Paper
// Z = C = Scissors