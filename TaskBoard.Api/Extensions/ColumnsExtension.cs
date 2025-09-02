using TaskBoard.Api.Models;
using TaskBoard.Api.Repositories;

namespace TaskBoard.Extensions;

public static class ColumnsExtension
{
    public static IEndpointRouteBuilder MapColumnEndpoints(this IEndpointRouteBuilder app)
    {
        // Seed default columns
        // using (var scope = app.Services.CreateScope())
        // {
        //     var services = scope.ServiceProvider;
        //     var columnRepo = services.GetRequiredService<IColumnRepository>();

        //     // Seed default columns if not exist
        //     await columnRepo.AddAsync(new Column { Name = "To Do" }, CancellationToken.None);
        //     await columnRepo.AddAsync(new Column { Name = "In Progress" }, CancellationToken.None);
        //     await columnRepo.AddAsync(new Column { Name = "Done" }, CancellationToken.None);
        // }
        app.MapGet("/columns", async (IColumnRepository repo, CancellationToken ct) => Results.Ok(await repo.GetAllAsync(ct)))
        .RequireAuthorization();

        app.MapPost("/columns", async (IColumnRepository repo, Column input, CancellationToken ct) =>
        {
            var c = new Column { Id = input.Id == Guid.Empty ? new Guid() : input.Id, Name = input.Name };
            await repo.AddAsync(c, ct);
            return Results.Created($"/columns/{c.Id}", c);
        })
        .RequireAuthorization();

        app.MapDelete("/column/{id:guid}", async (IColumnRepository repo, string id, CancellationToken ct) =>
        {
            await repo.DeleteAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization();

        app.MapGet("/column/{id:guid}", async (string id, IColumnRepository repo, CancellationToken ct) => Results.Ok(await repo.GetAsync(id, ct)))
        .RequireAuthorization();

        return app;
    }
}
