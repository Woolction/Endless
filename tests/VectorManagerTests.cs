using Recommendation.Managers;

namespace Tests;

public class VectorManagerTests
{
    [Fact]
    public void Normalize_Float_CorrectNormalizing()
    {
        float[] values = [
            100, 20, 1000
        ];

        VectorManager.Normalize(
            values, 3, x => x,
            (i, value) =>
            values[i] = value);

        float[] expected = [
            0.099f, 0.019f, 0.994f
        ];

        for (int i = 0; i < expected.Length; i++)
        {
            Console.WriteLine($"[VectorManagerTests] value: {values[i]}");
            Console.WriteLine($"[VectorManagerTests] expected: {expected[i]}");

            float abs = Math.Abs(expected[i] - values[i]);

            Console.WriteLine($"[VectorManagerTests] abs result: {abs}");
            Console.WriteLine("[VectorManagerTests]");

            Assert.True(abs < 0.01f);
        }
    }
}
