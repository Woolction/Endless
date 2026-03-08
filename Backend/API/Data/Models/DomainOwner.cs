using System.ComponentModel.DataAnnotations.Schema;
using Backend.API.Data.Components;

namespace Backend.API.Data.Models;

public class DomainOwner
{
    public Guid UserId { get; set; }
    [ForeignKey(nameof(UserId))] public User? User { get; set; }

    public Guid DomainId { get; set; }
    [ForeignKey(nameof(DomainId))] public Domain? Domain { get; set; }

    public DateTime OwnedDate { get; set; }
    public DomainOwnerRole OwnerRole { get; set; } = DomainOwnerRole.ContentEditor;
}
