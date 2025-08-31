using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Api.Models;
using TaskBoard.Api.Repositories;
using TaskBoard.Api.Services;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Taskboard API",
        Version = "v1",
        Description = "API documentation with Swagger"
    });
    // Bearer token input in Swagger "Authorize" button
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// Config for Azure Blob (env vars or appsettings)
string? blobConn = builder.Configuration["Azure:Blob:ConnectionString"];
string? blobContainer = builder.Configuration["Azure:Blob:Container"] ?? "task-images";
// Console.WriteLine("blobConn" + blobConn);
// Console.WriteLine("blobContainer" + blobContainer);

// DI
// builder.Services.AddSingleton<IColumnRepository, InMemoryColumnRepository>();
// builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
var connstring = builder.Configuration["Azure:DB:ConnectionString"];
Console.WriteLine("connstring: " + connstring);
builder.Services.AddDbContext<TaskBoardDbContext>(options =>
    options.UseSqlServer(connstring));

IImageStorage imageStorage = new AzureBlobImageStorage(blobConn, blobContainer);
 builder.Services.AddSingleton<IImageStorage>(imageStorage);
// builder.Services.AddScoped<ITaskRepository, EfTaskRepository>();
// builder.Services.AddScoped<ITaskService, TaskService>();
// builder.Services.AddScoped<IColumnRepository, EfColumnRepository>();

builder.Services.AddScoped<IColumnRepository, EfColumnRepository>();
builder.Services.AddScoped<ITaskRepository, EfTaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    var cfg = builder.Configuration;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = cfg["Jwt:Issuer"],
        ValidAudience = cfg["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!)
        ),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 🚀 Prevents infinite loops when serializing EF Core entities
        options.JsonSerializerOptions.ReferenceHandler = 
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
var app = builder.Build();
// Program.cs (add near your other endpoints)
app.MapPost("/auth/register", async (RegisterRequest req, TaskBoardDbContext db, IPasswordService pwd, ITokenService tokens, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Email and password are required.");

    var email = req.Email.Trim().ToLowerInvariant();
    if (await db.Users.AnyAsync(u => u.Email == email, ct))
        return Results.Conflict("Email already registered.");

    var user = new AppUser
    {
        Email = email,
        PasswordHash = pwd.Hash(req.Password),
        DisplayName = req.DisplayName
    };
    db.Users.Add(user);
    await db.SaveChangesAsync(ct);

    var tr = tokens.CreateTokens(user);

    // store refresh token hash
    db.RefreshTokens.Add(new RefreshToken
    {
        UserId = user.Id,
        TokenHash = tokens.Hash(tr.RefreshToken),
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(int.Parse(builder.Configuration["Jwt:RefreshTokenDays"]!))
    });
    await db.SaveChangesAsync(ct);

    return Results.Ok(tr);
})
.WithTags("Auth");

app.MapPost("/auth/login", async (LoginRequest req, TaskBoardDbContext db, IPasswordService pwd, ITokenService tokens, CancellationToken ct) =>
{
    var email = (req.Email ?? "").Trim().ToLowerInvariant();
    var user = await db.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == email, ct);
    if (user is null || !pwd.Verify(user.PasswordHash, req.Password))
        return Results.Unauthorized();

    var tr = tokens.CreateTokens(user);

    // invalidate oldest/expired tokens optionally here…

    db.RefreshTokens.Add(new RefreshToken
    {
        UserId = user.Id,
        TokenHash = tokens.Hash(tr.RefreshToken),
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(int.Parse(builder.Configuration["Jwt:RefreshTokenDays"]!))
    });
    await db.SaveChangesAsync(ct);

    return Results.Ok(tr);
})
.WithTags("Auth");

app.MapPost("/auth/refresh", async (RefreshRequest req, TaskBoardDbContext db, ITokenService tokens, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.RefreshToken)) return Results.BadRequest("Missing refresh token.");
    var hash = tokens.Hash(req.RefreshToken);

    var token = await db.RefreshTokens.Include(rt => rt.User)
        .FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);

    if (token is null || !token.IsActive)
        return Results.Unauthorized();

    // rotate: revoke old and issue new
    token.RevokedAt = DateTimeOffset.UtcNow;

    var tr = tokens.CreateTokens(token.User);
    var newHash = tokens.Hash(tr.RefreshToken);
    db.RefreshTokens.Add(new RefreshToken
    {
        UserId = token.UserId,
        TokenHash = newHash,
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(int.Parse(builder.Configuration["Jwt:RefreshTokenDays"]!)),
        ReplacedByTokenHash = null
    });
    await db.SaveChangesAsync(ct);

    return Results.Ok(tr);
})
.WithTags("Auth");

app.MapPost("/auth/revoke", async (RefreshRequest req, TaskBoardDbContext db, ITokenService tokens, CancellationToken ct) =>
{
    var hash = tokens.Hash(req.RefreshToken);
    var token = await db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);
    if (token is null) return Results.NotFound();
    token.RevokedAt = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
})
.WithTags("Auth");

// Register global exception handler
app.UseMiddleware<ExceptionHandlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Taskboard v1");
        c.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

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

app.MapGet("/tasks", async (ITaskService taskService, CancellationToken ct) =>
{
    var tasks = await taskService.GetAllAsync(ct);
    return Results.Ok(tasks);
});

app.MapPost("/tasks", async (ITaskService taskService, [FromBody] CreateTaskDto dto, CancellationToken ct) =>
{
    var t = await taskService.CreateAsync(dto, ct);
    var taskDto = new TaskDto(t.Id, t.Name, t.Description, t.Deadline , t.ColumnId, t.IsFavorite);
    return Results.Created($"/tasks/{t.Id}", taskDto);
})
.RequireAuthorization();


app.MapGet("/tasks/{id:guid}", async (ITaskService taskService, Guid id, CancellationToken ct)
    => (await taskService.GetAsync(new TaskId(id), ct)) is { } t ? Results.Ok(t) : Results.NotFound())
    .RequireAuthorization();


app.MapPut("/tasks/{id:guid}", async (ITaskService taskService, Guid id, EditTaskDto dto, CancellationToken ct) =>
{
    await taskService.EditAsync(new TaskId(id), dto, ct);
    return Results.NoContent();
}).RequireAuthorization();


app.MapDelete("/tasks/{id:guid}", async (ITaskService taskService, Guid id, CancellationToken ct) =>
{
    await taskService.DeleteAsync(new TaskId(id), ct);
    return Results.NoContent();
}).RequireAuthorization();


app.MapPost("/tasks/columnswitch", async ([FromBody] ColumnsSwitchDto columnsSwitchDto, ITaskService taskService, CancellationToken ct) =>
{   
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
    async (Guid id, IFormFileCollection files, ITaskService taskService, CancellationToken ct) =>
    {
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
 // 👈 disables antiforgery for this endpoint
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.Run();

