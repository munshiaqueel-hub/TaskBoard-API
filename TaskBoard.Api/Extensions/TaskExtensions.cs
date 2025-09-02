using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TaskBoard.Api.Models;
using TaskBoard.Api.Services;

namespace TaskBoard.Extensions;
public static class TasksExtensions
{
    public static async Task<IResult> Validate<T>(T dto, IValidator<T> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            Console.WriteLine("validationResult: " + validationResult.IsValid);
            return Results.BadRequest(validationResult.Errors);
        }
        return Results.NoContent();
    }

    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks", async (ITaskService taskService, CancellationToken ct) =>
        {
            var tasks = await taskService.GetAllAsync(ct);
            return Results.Ok(tasks);
        })
        .RequireAuthorization();

        app.MapPost("/tasks", async (ITaskService taskService, [FromBody] CreateTaskDto dto,
        IValidator<CreateTaskDto> validator, CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }

            var t = await taskService.CreateAsync(dto, ct);
            var taskDto = new TaskDto(t.Id, t.Name, t.Description, t.Deadline, t.ColumnId, t.IsFavorite);
            return Results.Created($"/tasks/{t.Id}", taskDto);
        })
        .RequireAuthorization();


        app.MapGet("/tasks/{id:guid}", async (ITaskService taskService, Guid id, CancellationToken ct)
            => (await taskService.GetAsync(new TaskId(id), ct)) is { } t ? Results.Ok(t) : Results.NotFound())
            .RequireAuthorization();


        app.MapPut("/tasks/{id:guid}", async (ITaskService taskService, Guid id, EditTaskDto dto,
        IValidator<EditTaskDto> validator, CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }
            await taskService.EditAsync(new TaskId(id), dto, ct);
            return Results.NoContent();
        }).RequireAuthorization();


        app.MapDelete("/tasks/{id:guid}", async (ITaskService taskService, Guid id, CancellationToken ct) =>
        {
            await taskService.DeleteAsync(new TaskId(id), ct);
            return Results.NoContent();
        }).RequireAuthorization();


        app.MapPost("/tasks/columnswitch", async ([FromBody] ColumnsSwitchDto columnsSwitchDto,IValidator<ColumnsSwitchDto> validator, ITaskService taskService, CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(columnsSwitchDto);

            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }
            await taskService.MoveAsync(new TaskId(columnsSwitchDto.Id), columnsSwitchDto.ColumnId.ToString(), ct);
            return Results.NoContent();
        }).RequireAuthorization();


        app.MapGet("/columns/{columnId:guid}/tasks", async (ITaskService taskService, Guid columnId, CancellationToken ct)
            => Results.Ok(await taskService.GetColumnTasksSortedAsync(columnId, ct)))
            .RequireAuthorization();

        // Single Upload at a time
        // app.MapPost("/tasks/{id:guid}/images", async (HttpRequest req, ITaskService taskService, Guid id, CancellationToken ct) =>
        // {
        //     if (!req.HasFormContentType) return Results.BadRequest("multipart/form-data expected");
        //     var form = await req.ReadFormAsync(ct);
        //     var file = form.Files.FirstOrDefault();
        //     if (file is null) return Results.BadRequest("file required");
        //     await using var stream = file.OpenReadStream();
        //     var url = await taskService.AttachImageAsync(new TaskId(id), file.FileName, stream, file.ContentType, ct);
        //     return Results.Ok(new { Url = url });
        // });

        app.MapPost("/tasks/{id:guid}/images",
            async (Guid id, IFormFileCollection files, ITaskService taskService, IValidator<IFormFileCollection> validator, CancellationToken ct) =>
            {
                 var validationResult = await validator.ValidateAsync(files);

                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(validationResult.Errors);
                }

                if (files == null || files.Count == 0)
                    return Results.BadRequest("At least one file is required");

                var urls = await taskService.AttachImageAsync(new TaskId(id), files, ct);
                return Results.Ok(new { Urls = urls });
            })
            .WithSummary("Upload images for a task")
            .WithDescription("Uploads one or more images (multipart/form-data) and attaches them to the given task")
            .Accepts<IFormFileCollection>("multipart/form-data")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
        .RequireAuthorization();
        
        return app;
    }
}
