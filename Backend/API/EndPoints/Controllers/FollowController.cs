using Backend.API.Data.Context;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FollowController : ControllerBase
{
    private readonly EndlessContext context;

    public FollowController(EndlessContext context)
    {
        this.context = context;        
    }
}