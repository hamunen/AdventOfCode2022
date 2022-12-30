using System;

namespace AdventOfCode.Tests
{
    public class Day25Tests
    {

        public Day25Tests()
        {
        }

        [Fact]
        public void SnafuAddition_Simple()
        {
            var a = new Snafu(new[] { '1' });
            var b = new Snafu(new[] { '1' });

            var result = a + b;
            var expected = new[] { '2'};

            Assert.Equal(expected, result.Digits);
        }

        [Theory]
        [InlineData("1", "2", "1=")]
        [InlineData("2", "2", "1-")]
        [InlineData("2", "1", "1=")]
        [InlineData("0", "2", "2")]
        [InlineData("0", "0", "0")]

        public void SnafuAdditions_SinglePositiveDigits(string aStr, string bStr, string expStr)
        {
            var a = new Snafu(aStr.ToCharArray());
            var b = new Snafu(bStr.ToCharArray());
            Assert.Equal(expStr, (a + b).ToString());
        }

        [Theory]
        [InlineData("1", "-", "0")]
        [InlineData("2", "=", "0")]
        [InlineData("0", "-", "-")]
        [InlineData("1", "=", "-")]
        [InlineData("-", "-", "=")]
        [InlineData("-", "=", "-2")]
        [InlineData("=", "=", "-1")]

        public void SnafuAdditions_NegativeDigits(string aStr, string bStr, string expStr)
        {
            var a = new Snafu(aStr.ToCharArray());
            var b = new Snafu(bStr.ToCharArray());
            Assert.Equal(expStr, (a + b).ToString());
        }

        [Theory]
        [InlineData("1=", "10", "2=")]
        [InlineData("2-", "2", "21")]
        [InlineData("2=", "12", "1=0")]
        [InlineData("20", "20", "1-0")]
        [InlineData("1121-1110-1=0", "1", "1121-1110-1=1")]
        public void SnafuAdditions_MultipleDigits(string aStr, string bStr, string expStr)
        {
            var a = new Snafu(aStr.ToCharArray());
            var b = new Snafu(bStr.ToCharArray());
            Assert.Equal(expStr, (a + b).ToString());
        }



    }
}
