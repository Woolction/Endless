using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.API.Data.Models;

public class ContentGenreVector
{
    public Guid ContentId { get; set; }
    public Content? Content { get; set; }
    public Guid GenreId { get; set; }
    public Genre? Genre { get; set; }

    public float AuthorVector { get; set; }
    public float AudienceVector { get; set; }
    public float FinalVector { get; set; }
}