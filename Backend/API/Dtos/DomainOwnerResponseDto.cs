using Backend.API.Data.Components;

namespace Backend.API.Dtos;

public record class DomainOwnerResponseDto(
    Guid OwnerId,
    Guid DomainId,
    DateTime OwnedDate,
    DomainOwnerRole OwnerRole
    );