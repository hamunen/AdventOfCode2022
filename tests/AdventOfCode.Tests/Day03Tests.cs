namespace AdventOfCode.Tests
{
    public class Day03Tests
    {
        [Theory]
        [InlineData("vJrwpWtwJgWrhcsFMMfFFhFp", 'p')]
        [InlineData("jqHRNqRjqzjGDLGLrsFMfFZSrLrFZsSL", 'L')]
        [InlineData("PmmdzqPrVvPwwTWBwg", 'P')]
        [InlineData("wMqvLMZHhHMvwLHjbvcjnnSBnvTQFn", 'v')]
        [InlineData("ttgJtRGJQctTZtZT", 't')]
        [InlineData("CrZsJsPPZsGzwwsLwLmpwMDw", 's')]
        public void FindDuplicateItem(string rucksackInput, char expectedDuplicate)
        {
            var rucksack = new Rucksack(rucksackInput);
            var result = rucksack.FindDuplicateItem();

            Assert.Equal(expectedDuplicate, result);
        }

        [Theory]
        [InlineData('p', 16)]
        [InlineData('L', 38)]
        [InlineData('P', 42)]
        [InlineData('v', 22)]
        [InlineData('t', 20)]
        [InlineData('s', 19)]
        public void GetItemPriority_GetsPriority(char item, char expectedPriority)
        {
            var result = Rucksack.GetItemPriority(item);
            Assert.Equal(expectedPriority, result);
        }

        [Theory]
        [InlineData("vJrwpWtwJgWrhcsFMMfFFhFp", 16)]
        [InlineData("jqHRNqRjqzjGDLGLrsFMfFZSrLrFZsSL", 38)]
        [InlineData("PmmdzqPrVvPwwTWBwg", 42)]
        [InlineData("wMqvLMZHhHMvwLHjbvcjnnSBnvTQFn", 22)]
        [InlineData("ttgJtRGJQctTZtZT", 20)]
        [InlineData("CrZsJsPPZsGzwwsLwLmpwMDw", 19)]
        public void GetDuplicateItemPriority(string rucksackInput, int expectedPriority)
        {
            var rucksack = new Rucksack(rucksackInput);
            var result = rucksack.GetDuplicateItemPriority();

            Assert.Equal(expectedPriority, result);
        }

        [Fact]
        public void GetPriorityOfCommonItemInRucksacks()
        {
            var rucksackGroup = new Rucksack[]
            {
                new Rucksack("vJrwpWtwJgWrhcsFMMfFFhFp"),
                new Rucksack("jqHRNqRjqzjGDLGLrsFMfFZSrLrFZsSL"),
                new Rucksack("PmmdzqPrVvPwwTWBwg")
            };

            var result = Rucksack.GetPriorityOfCommonItemInRucksacks(rucksackGroup);
            Assert.Equal(18, result);
        }
    }
}


