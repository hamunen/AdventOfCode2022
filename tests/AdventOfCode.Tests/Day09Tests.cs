namespace AdventOfCode.Tests
{
    public class Day09Tests
    {
        [Theory]
        [InlineData(5, 5, 4, 4, 4, 4)]
        [InlineData(3,2,1,1,2,2)]
        [InlineData(2, 3, 1, 1, 2, 2)]
        public void GetClosestAdjacentPosition(int headX, int headY, int tailX, int tailY
            , int expectedX, int expectedY)
        {
            var head = new Position(headX, headY);
            var tail = new Position(tailX, tailY);

            var result = tail.GetClosestAdjacentPosition(head);
            var expected = new Position(expectedX, expectedY);
            Assert.Equal(expected, result);
        }
    }
}
