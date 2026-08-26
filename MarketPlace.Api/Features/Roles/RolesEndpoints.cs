using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.DatabaseContext.SeedData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Roles;

public class RolesEndpoints : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        builder = builder.MapGroup("roles").RequireAuthorization().WithTags("Roles");

        builder
            .MapGet(
                "/",
                async ([FromServices] AppDbContext context, CancellationToken ct) =>
                    Results.Ok(
                        await context
                            .Roles.Select(s => new RoleResponse(s.Id, s.Name))
                            .ToListAsync(ct)
                    )
            )
            .Produces<List<RoleResponse>>()
            .RequireAuthorization(p =>
                p.RequireRole(CustomAppRoles.SuperAdmin, CustomAppRoles.Admin)
            );
    }
}

public sealed record RoleResponse(Guid Id, string Name);
