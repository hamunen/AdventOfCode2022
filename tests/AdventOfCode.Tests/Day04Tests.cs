namespace AdventOfCode.Tests
{
    public class Day04Tests
    {
        [Theory]
        [InlineData("2-4,6-8")]
        [InlineData("50-55,41-48")]
        public void ParseSectionsFrom_ParsesInputAndReturnsPair(string inputLine)
        {
            var result = SectionAssignmentPair.ParseSectionsFrom(inputLine);
            Assert.IsType<SectionAssignmentPair>(result);
        }

        [Theory]
        [InlineData("2-4,6-8")]
        [InlineData("50-55,41-48")]
        [InlineData("1-2,3-4")]
        public void OneSectionFullyContainsOther_FullyNonOverlappingPair_ReturnsFalse(string inputLine)
        {
            var pair = SectionAssignmentPair.ParseSectionsFrom(inputLine);

            var result = pair.OneSectionFullyContainsOther();
            Assert.False(result);
        }

        [Theory]
        [InlineData("5-7,7-8")]
        [InlineData("2-6,4-8")]
        [InlineData("1-2,2-3")]
        public void OneSectionFullyContainsOther_PartiallyOverlappingPair_ReturnsFalse(string inputLine)
        {
            var pair = SectionAssignmentPair.ParseSectionsFrom(inputLine);

            var result = pair.OneSectionFullyContainsOther();
            Assert.False(result);
        }

        [Theory]
        [InlineData("2-8,3-7")]
        [InlineData("3-7,2-8")]
        [InlineData("6-6,4-6")]
        [InlineData("100-200,101-199")]
        [InlineData("100-200,100-200")]
        public void OneSectionFullyContainsOther_FullyOverlappingPair_ReturnsTrue(string inputLine)
        {
            var pair = SectionAssignmentPair.ParseSectionsFrom(inputLine);

            var result = pair.OneSectionFullyContainsOther();
            Assert.True(result);
        }


        [Theory]
        [InlineData("2-4,6-8")]
        [InlineData("50-55,41-48")]
        [InlineData("1-2,3-4")]
        public void RangesOverlap_FullyNonOverlappingPair_ReturnsFalse(string inputLine)
        {
            var pair = SectionAssignmentPair.ParseSectionsFrom(inputLine);

            var result = pair.RangesOverlap();
            Assert.False(result);
        }

        [Theory]
        [InlineData("5-7,7-8")]
        [InlineData("2-6,4-8")]
        [InlineData("1-2,2-3")]
        public void RangesOverlap_PartiallyOverlappingPair_ReturnsTrue(string inputLine)
        {
            var pair = SectionAssignmentPair.ParseSectionsFrom(inputLine);

            var result = pair.RangesOverlap();
            Assert.True(result);
        }

        [Theory]
        [InlineData("2-8,3-7")]
        [InlineData("3-7,2-8")]
        [InlineData("6-6,4-6")]
        [InlineData("100-200,101-199")]
        [InlineData("100-200,100-200")]
        public void RangesOverlap_FullyOverlappingPair_ReturnsTrue(string inputLine)
        {
            var pair = SectionAssignmentPair.ParseSectionsFrom(inputLine);

            var result = pair.RangesOverlap();
            Assert.True(result);
        }
    }
}
