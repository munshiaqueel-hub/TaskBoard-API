using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http;
using TaskBoard.Api.Services;
using FluentValidation;
using TaskBoard.Api.Models;

class TaskApiFactory : WebApplicationFactory<Program> // Program is your entry point class
{
    public Mock<ITaskService> TaskServiceMock { get; } = new();
    public Mock<IValidator<CreateTaskDto>> CreateTaskValidatorMock { get; } = new();
    public Mock<IValidator<EditTaskDto>> EditTaskValidatorMock { get; } = new();
    public Mock<IValidator<IFormFileCollection>> FileCollectionValidatorMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing service registrations
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITaskService));
            if (descriptor != null) services.Remove(descriptor);

            // Re-register with mocks
            services.AddSingleton(TaskServiceMock.Object);
            services.AddSingleton(CreateTaskValidatorMock.Object);
            services.AddSingleton(EditTaskValidatorMock.Object);
            services.AddSingleton(FileCollectionValidatorMock.Object);
        });
    }
}
