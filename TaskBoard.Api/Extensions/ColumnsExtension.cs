using FluentValidation;
using TaskBoard.Api.Models;
using TaskBoard.Api.Repositories;

namespace TaskBoard.Extensions;

public static class ColumnsExtension
{
    public static IEndpointRouteBuilder MapColumnEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/columns", async (IColumnService columnService, CancellationToken ct) => Results.Ok(await columnService.GetAllAsync(ct)))
        .RequireAuthorization();
 
        app.MapPost("/columns", async (IColumnService columnService,
            IValidator<CreateColumnDto> validator, CreateColumnDto input, CancellationToken ct) =>
        {
             var validationResult = await validator.ValidateAsync(input);

            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }
            var c = await columnService.AddColumn(input, ct);
            return Results.Created($"/columns/{c.Id}", c);
        })
        .RequireAuthorization();

        app.MapDelete("/column/{id:guid}", async (IColumnService columnService, IColumnRepository repo, string id, CancellationToken ct) =>
        {
            await columnService.DeleteColumn(id, ct);
            return Results.NoContent();
        }).RequireAuthorization();

        app.MapGet("/column/{id:guid}", async (string id, IColumnService columnService, CancellationToken ct) => Results.Ok(await columnService.GetByIdAsync(id, ct)))
        .RequireAuthorization();

        return app;
    }
}
