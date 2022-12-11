using AoCHelper;
using Xunit;

namespace AdventOfCode.Tests
{
    public class Day11Tests
    {
        private readonly string _testInput1;
        private readonly Monkey _testMonkey0;
        private readonly Monkey _testMonkey1;
        private readonly Monkey _testMonkey2;
        private readonly Monkey _testMonkey3;
        private readonly MonkeyBusiness _business;

        public Day11Tests()
        {
            var path = Path.Combine("TestInputs", "Day11_1.txt");
            _testInput1 = File.ReadAllText(path);

            _business = new MonkeyBusiness(_testInput1);
            _testMonkey0 = _business.Monkeys[0];
            _testMonkey1 = _business.Monkeys[1];
            _testMonkey2 = _business.Monkeys[2];
            _testMonkey3 = _business.Monkeys[3];
        }

        [Fact]
        public void ParseFromInput_CorrectItems()
        {
            Assert.Equal(79, _testMonkey0.Items.ElementAt(0));
            Assert.Equal(98, _testMonkey0.Items.ElementAt(1));
        }

        [Fact]
        public void ParseFromInput_CorrectMultiplyOperation()
        {
            // operation is * 19
            var result = _testMonkey0.Operation(10);
            Assert.Equal(190, result);
        }

        [Fact]
        public void ParseFromInput_CorrectSelfMultiplyOperation()
        {
            var result = _testMonkey2.Operation(8);
            Assert.Equal(64, result);
        }

        [Fact]
        public void ParseFromInput_CorrectAddOperation()
        {
            // operation is + 6
            var result = _testMonkey1.Operation(10);
            Assert.Equal(16, result);
        }

        [Fact]
        public void ParseFromInput_CorrectTest()
        {
            // test is 'divisible by 23'
            Assert.True(_testMonkey0.Test(23));
            Assert.False(_testMonkey0.Test(32));
        }

        [Fact]
        public void ParseFromInput_CorrectThrowIfTrue()
        {
            Assert.Equal(2, _testMonkey0.MonkeyToThrowToIfTestTrue);
        }

        [Fact]
        public void ParseFromInput_CorrectThrowIfFalse()
        {
            Assert.Equal(3, _testMonkey0.MonkeyToThrowToIfTestFalse);
        }

        [Fact]
        public void Run_OneRound_MonkeysHaveCorrectAmountOfItems()
        {
            _business.Run(1);
            Assert.Equal(4, _testMonkey0.Items.Count);
            Assert.Equal(6, _testMonkey1.Items.Count);
            Assert.Empty(_testMonkey2.Items);
            Assert.Empty(_testMonkey3.Items);
        }

        [Fact]
        public void Run_OneRound_MonkeysHaveCorrectItems()
        {
            _business.Run(1);
            Assert.Equal(20, _testMonkey0.Items.ElementAt(0));
            Assert.Equal(1046, _testMonkey1.Items.ElementAt(5));
        }

        [Fact]
        public void Run_20Rounds_MonkeysHaveCorrectItems()
        {
            _business.Run(20);
            Assert.Equal(10, _testMonkey0.Items.ElementAt(0));
            Assert.Equal(199, _testMonkey1.Items.ElementAt(3));
        }

        [Fact]
        public void GetLevelOfMonkeyBusiness_After20Rounds()
        {
            _business.Run(20);
            var result = _business.GetLevelOfMonkeyBusiness();
            Assert.Equal<long>(10605, result);
        }

        [Fact]
        public void InspectedItems_NoRelief_After1Round()
        {
            var business = new MonkeyBusiness(_testInput1, false);
            business.Run(1);
            var result = business.Monkeys[0].InspectCounter;
            Assert.Equal(2, result);
        }

        [Theory]
        [InlineData(20, 97)]
        [InlineData(1000, 4792)]
        [InlineData(6000, 28702)]
        [InlineData(10000, 47830)]
        public void InspectedItems_NoRelief_AfterXRounds(
            int rounds, int expected)
        {
            var business = new MonkeyBusiness(_testInput1, false);
            business.Run(rounds);
            var result = business.Monkeys[1].InspectCounter;
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetLevelOfMonkeyBusiness_NoRelief_After10000Rounds()
        {
            var business = new MonkeyBusiness(_testInput1, false);
            business.Run(10000);
            var result = business.GetLevelOfMonkeyBusiness();
            Assert.Equal<long>(2713310158, result);
        }


    }
}
