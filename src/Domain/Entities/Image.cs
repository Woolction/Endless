namespace Domain.Entities;

public class Image
{
    public Guid Id { get; set; }

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public required string BaseUrl { get; set; }

    public List<ImageVariant> Variants { get; set; } = [];

    public void SetColor(int r, int g, int b)
    {
        R = r;
        G = g;
        B = b;
    }
}