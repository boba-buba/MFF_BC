using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
BenchmarkRunner.Run<FactorialBenchmarks>();
public class FactorialBenchmarks
{
    // Variant Alpha:
    public static int CalcFactorialAlpha(int sourceN)
    {
        if (sourceN == 0) return 1;
        if (sourceN == 1) return 1;
        if (sourceN < 0) throw new ArgumentOutOfRangeException(nameof(sourceN), sourceN, "Should benon - negative.");
    try
        {
            int resultOfNFactorial = 1;
            for (int i = 1; i <= sourceN; i++)
            {
                checked { resultOfNFactorial *= i; }
            }
            return resultOfNFactorial;
        }
        catch (OverflowException)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceN), sourceN, "Too big for Int32result.");
        }
    }
    // Variant Bravo:
    public static int CalcFactorialBravo(int sourceN)
    {
        if (sourceN == 0) return 1;
        if (sourceN == 1) return 1;
        return CalcFactorialBravoInternal(sourceN);
    }
    private static int CalcFactorialBravoInternal(int sourceN)
    {
        if (sourceN < 0) throw new ArgumentOutOfRangeException(nameof(sourceN), sourceN, "Should benon - negative.");
    try
        {
            int resultOfNFactorial = 1;
            for (int i = 1; i <= sourceN; i++)
            {
                checked { resultOfNFactorial *= i; }
            }
            return resultOfNFactorial;
        }
        catch (OverflowException)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceN), sourceN, "Too big for Int32result.");
        }
    }
    // Actual benchmarks:
    [Benchmark]
    public int Alpha0() => CalcFactorialAlpha(0);
    [Benchmark]
    public int Bravo0() => CalcFactorialBravo(0);
    [Benchmark]
    public int Alpha2() => CalcFactorialAlpha(2);
    [Benchmark]
    public int Bravo2() => CalcFactorialBravo(2);
}