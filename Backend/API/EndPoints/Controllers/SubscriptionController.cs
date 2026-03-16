using Backend.API.Data.Context;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly EndlessContext context;

    public SubscriptionController(EndlessContext context)
    {
        this.context = context;
    }
}