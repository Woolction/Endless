using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Contents.Create;

public class ContentCreateHandler : IRequestHandler<ContentCreateCommand>
{
    private readonly IAppDbContext context;

    public ContentCreateHandler(IAppDbContext context)
    {
        this.context = context;
    }

    public async Task Handle(ContentCreateCommand cmd, CancellationToken cancellationToken)
    {
        
    }
}