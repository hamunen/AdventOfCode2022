using AoCHelper;
using Xunit;

namespace AdventOfCode.Tests
{
    public class Day20Tests
    {
        private readonly EncryptedFile _testFile;

        public Day20Tests()
        {
            //var path = Path.Combine("TestInputs", "Day11_1.txt");
            //_testInput1 = File.ReadAllText(path);

            _testFile = new EncryptedFile("1\r\n2\r\n-3\r\n3\r\n-2\r\n0\r\n4");

        }

        [Fact]
        public void MoveValues_MovesRight()
        {
            var list = new List<(int, bool)>
            {
                (1, true), (2, false), (3, false)
                , (4, false), (5, false)
            };

            EncryptedFile.MoveValue(list, 1);
            Assert.Equal(2, list[3].Item1);
        }

        [Fact]
        public void MoveValues_MovesLeft()
        {
            var list = new List<(int, bool)>
            {
                (1, true), (2, true), (3, true), (-2, false), (5, false)
            };

            EncryptedFile.MoveValue(list, 3);
            Assert.Equal(-2, list[1].Item1);
        }

        [Fact]
        public void MoveValues_MovesLeft_OtherPositionsAreCorrect()
        {
            var list = new List<(int, bool)>
            {
                (1, true), (2, true), (3, true), (-2, false), (5, false)
            };

            EncryptedFile.MoveValue(list, 3);
            Assert.Equal(3, list[3].Item1);
        }

        [Fact]
        public void MoveValues_MovesRightOverTheEnd_WrapsAround()
        {
            var list = new List<(int, bool)>
            {
                (1, true), (2, false), (3, false), (4, false)
                , (5, false)
            };

            EncryptedFile.MoveValue(list, 3);
            Assert.Equal(4, list[3].Item1);
        }

        [Fact]
        public void MoveValues_MovesRightOverTheEnd_WrapsAround2()
        {
            var list = new List<(int, bool)>
            {
            (1, false), (2, false), (-3, false), (3, false)
            , (-2, false), (0, false), (4, false)            
            };

            EncryptedFile.MoveValue(list, 6);
            Assert.Equal(4, list[4].Item1);
        }

        [Fact]
        public void MoveValues_MovesRightToLastPos_WrapsAroundToStart()
        {
            var list = new List<(int, bool)>
            {
            (1, false), (2, false), (-3, false), (3, false)
            , (-2, false), (0, false), (4, false)
            };

            EncryptedFile.MoveValue(list, 3);
            Assert.Equal(3, list[0].Item1);
        }

        [Fact]
        public void MoveValues_MovesRightByLenMinus1()
        {
            // 10 length
            var list = new int[] { 1, 2, -3, 3, -2, 0, 9, 8, 2, -6 }
                .Select(i => (i, false)).ToList();

            EncryptedFile.MoveValue(list, 6);
            Assert.Equal(9, list[6].Item1);
        }

        [Fact]
        public void MoveValues_MovesLeftByLenMinus1()
        {
            // 10 length
            var list = new int[] { 1, 2, -3, 3, -2, 0, -9, 8, 2, -6 }
                .Select(i => (i, false)).ToList();

            EncryptedFile.MoveValue(list, 6);
            Assert.Equal(-9, list[6].Item1);
        }

        [Fact]
        public void MoveValues_MovesLeftOverTheEnd_WrapsAround()
        {
            var list = new List<(int, bool)>
            {
            (1, false), (2, false), (-3, false), (3, false)
            , (-2, false), (0, false), (4, false)
            };

            EncryptedFile.MoveValue(list, 2);
            Assert.Equal(-3, list[5].Item1);
        }

        [Fact]
        public void MoveValues_MovesLeftToZeroPos_WrapsAroundToEnd()
        {
            var list = new List<(int, bool)>
            {
            (1, false), (2, false), (-2, false), (3, false)
            , (-2, false), (0, false), (4, false)
            };

            EncryptedFile.MoveValue(list, 2);
            Assert.Equal(-2, list[6].Item1);
        }

        [Fact]
        public void MoveValues_MovesLeftFromZeroPos()
        {
            var list = new List<(int, bool)>
            {
            (-3, false), (2, false), (-2, false), (3, false)
            , (-2, false), (0, false), (4, false)
            };

            EncryptedFile.MoveValue(list, 0);
            Assert.Equal(-3, list[3].Item1);
        }

        [Fact]
        public void MoveValues_MovesLeftWithMoreThanLength()
        {
            // len 7
            var list = new List<(int, bool)>
            {
            (2, false), (-10, false), (-2, false), (3, false)
            , (-2, false), (0, false), (4, false)
            };

            // after 3
            EncryptedFile.MoveValue(list, 1);
            Assert.Equal(-10, list[3].Item1);
        }


        [Fact]
        public void MoveValues_MovesLeftFromZeroPosWithMoreThanLength()
        {
            // len 7
            var list = new List<(int, bool)>
            {
            (-10, false), (2, false), (-2, false), (3, false)
            , (-2, false), (0, false), (4, false)
            };

            // after -2
            EncryptedFile.MoveValue(list, 0);
            Assert.Equal(-10, list[2].Item1);
        }


        [Fact]
        public void MixFile()
        {
            var result = _testFile.MixFile();
            var resultString = string.Join(", ", result);
            var expected = "1, 2, -3, 4, 0, 3, -2";
            Assert.Equal(expected, resultString);
        }

        [Theory]
        [InlineData(1000, 4)]
        [InlineData(2000, -3)]
        [InlineData(3000, 2)]
        public void FindNthnumberAfterZer_Finds1000thNumber(int n, int expected)
        {
            var numbers = new int[] { 1, 2, -3, 4, 0, 3, -2 };
            var result = EncryptedFile.FindNthNumberAfterZero(numbers, n);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MixFile_2()
        {
            var list = new int[] { 3, 2, 1, 0 };
            var str = string.Join("\r\n", list);

            var file = new EncryptedFile(str);
            var result = file.MixFile();
            var resultString = string.Join(", ", result);
            var expected = "1, 2, 3, 0";
            Assert.Equal(expected, resultString);
        }

        [Fact]
        public void MixFile_3()
        {
            var list = new int[] { 1, 2, 3, 0 };
            var str = string.Join("\r\n", list);

            var file = new EncryptedFile(str);
            var result = file.MixFile();
            var resultString = string.Join(", ", result);
            var expected = "1, 3, 2, 0";
            Assert.Equal(expected, resultString);
        }

        [Fact]
        public void MixFile_4()
        {
            // 8 length
            var list = new int[] { 1, 1, 1, 1, 1, 0, 1, 1 };
            var str = string.Join("\r\n", list);

            var file = new EncryptedFile(str);
            var result = file.MixFile();
            var resultString = string.Join(", ", result);
            var expected = "1, 1, 1, 1, 1, 1, 0, 1";
            Assert.Equal(expected, resultString);
        }
    }
}
