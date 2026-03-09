using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Models;
using Backend.API.Dtos;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DomainOwnersController : ControllerBase
{
    private readonly EndlessContext context;

    public DomainOwnersController(EndlessContext context)
    {
        this.context = context;
    }
    
    [HttpGet("{DomainId}")]
    public async Task<IActionResult> GetDomainOwners(Guid DomainId)
    {
        List<DomainOwnerResponseDto> domainOwners = await context.DomainOwners
        .Where(owner => owner.DomainId == DomainId)
        .Select(owner => new DomainOwnerResponseDto(
            owner.OwnerId, owner.DomainId, owner.OwnedDate,
            owner.OwnerRole)).AsNoTracking().ToListAsync();

        return Ok(domainOwners);
    }
}