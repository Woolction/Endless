namespace Backend.API.Managers;

public static class VectorManager
{
    public static float Dot<T, B>(T[] a, B[] b, long count, Func<T, float> selector, Func<B, float> selectorB)
    {
        float sum = 0;
        for (int i = 0; i < count; i++)
            sum += selector(a[i]) * selectorB(b[i]);

        return sum;
    }

    public static float Magnitude<T>(T[] v, Func<T, float> selector)
    {
        float sum = 0;

        foreach (var x in v)
            sum += selector(x) * selector(x);

        return MathF.Sqrt(sum);
    }

    public static float CosineSimilarity<T, B>(T[] a, B[] b, int count, Func<T, float> selector, Func<B, float> selectorB)
    {
        float magT = Magnitude(a, selector);
        float magB = Magnitude(b, selectorB);

        if (magT == 0 || magB == 0)
            return 0;

        return Dot(a, b, count, selector, selectorB) /
            (magT * magB);
    }

    public static float CosineSimilarityNormalized<T, B>(T[] a, B[] b, long count, Func<T, float> sa, Func<B, float> sb)
    {
        float sum = 0;

        for (int i = 0; i < count; i++)
            sum += sa(a[i]) * sb(b[i]);

        return sum;
    }

    public static void Normalize<T>(T[] v, long count, Func<T, float> getter, Action<T, float> setter)
    {
        float mag = Magnitude(v, getter);
        if (mag == 0) return;

        for (int i = 0; i < count; i++)
        {
            var value = getter(v[i]);
            setter(v[i], value / mag);
        }
    }
}