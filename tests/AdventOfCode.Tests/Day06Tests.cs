namespace AdventOfCode.Tests
{
    public class Day06Tests
    {
        [Theory]
        [InlineData("mjqjpqmgbljsphdztnvjfqwrcgsmlb", 7)]
        [InlineData("bvwbjplbgvbhsrlpgdmjqwftvncz", 5)]
        [InlineData("nppdvjthqldpwncqszvftbrmjlhg", 6)]
        [InlineData("nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg", 10)]
        [InlineData("zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw", 11)]
        public void GetStartOfPacketMarkerPosition(string input, int expected)
        {
            var dataStream = new ElfDataStream(input);

            var result = dataStream.GetStartOfPacketMarkerPosition();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("mjqjpqmgbljsphdztnvjfqwrcgsmlb", 19)]
        [InlineData("bvwbjplbgvbhsrlpgdmjqwftvncz", 23)]
        [InlineData("nppdvjthqldpwncqszvftbrmjlhg", 23)]
        [InlineData("nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg", 29)]
        [InlineData("zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw", 26)]
        public void GetStartOfMessageMarkerPosition(string input, int expected)
        {
            var dataStream = new ElfDataStream(input);

            var result = dataStream.GetStartOfMessageMarkerPosition();
            Assert.Equal(expected, result);
        }
    }
}
