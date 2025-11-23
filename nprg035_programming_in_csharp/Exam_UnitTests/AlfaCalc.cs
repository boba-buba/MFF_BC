using Exam;

namespace Exam_UnitTests
{
    public class AlfaCalc
    {

        [Fact]
        public void NegativeNumber_ArgOutOfRangeException()
        {
            // Arrange
            int InputNumber = -2;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => FactorialBenchmarks.CalcFactorialAlpha(InputNumber));
        }

        [Fact]
        public void Overflow_ArgOutOfRangeException()
        {
            //Arrange
            int InputNumber = int.MaxValue;
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => FactorialBenchmarks.CalcFactorialAlpha(InputNumber));
        }

        [Fact]
        public void PrettyInput()
        {
            //Arrange
            int InputNumber = 5;
            int ExpectedResult = 5*4*3*2;
            //Act
            int result = FactorialBenchmarks.CalcFactorialAlpha(InputNumber);
            Assert.Equal(ExpectedResult, result);
        }
    }
}