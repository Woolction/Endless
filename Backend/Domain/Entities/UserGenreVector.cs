namespace Domain.Entities;

public class UserGenreVector
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid GenreId { get; set; }
    public Genre? Genre { get; set; }

    public float Value { get; set; }
}