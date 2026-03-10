using System.ComponentModel.DataAnnotations.Schema;
using Backend.API.Data.Components;

namespace Backend.API.Data.Models;

public class DomainOwner
{
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }

    public Guid DomainId { get; set; }
    public Domain? Domain { get; set; }

    public DateTime OwnedDate { get; set; }
    public DomainOwnerRole OwnerRole { get; set; } = DomainOwnerRole.ContentEditor;
}
