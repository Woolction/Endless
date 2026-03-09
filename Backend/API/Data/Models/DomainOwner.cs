using System.ComponentModel.DataAnnotations.Schema;
using Backend.API.Data.Components;

namespace Backend.API.Data.Models;

public class DomainOwner
{
    public Guid OwnerId { get; set; }
    [ForeignKey(nameof(OwnerId))] public User? Owner { get; set; }

    public Guid DomainId { get; set; }
    [ForeignKey(nameof(DomainId))] public Domain? Domain { get; set; }

    public DateTime OwnedDate { get; set; }
    public DomainOwnerRole OwnerRole { get; set; } = DomainOwnerRole.ContentEditor;
}
