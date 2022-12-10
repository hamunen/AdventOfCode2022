using AoCHelper;

namespace AdventOfCode.Tests
{
    public class Day10Tests
    {
        private string _testInput1;
        
        public Day10Tests()
        {
            var path = Path.Combine("TestInputs", "Day10_1.txt");
            _testInput1 = File.ReadAllText(path);

        }

        [Fact]
        public void GetSignalStrengthDuringCycle_20()
        {
            var cpu = new CommunicationDeviceCPU();
            cpu.ProcessFullInstructionsInput(_testInput1);

            var result = cpu.GetSignalStrengthDuringCycle(20);
            Assert.Equal(420, result);
        }
    }
}
