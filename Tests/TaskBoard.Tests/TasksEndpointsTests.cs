// using System.Net;
// using FluentValidation.Results;
// using Moq;
// using NUnit.Framework;
// using TaskBoard.Api.Models;

// [TestFixture]
// public class TaskEndpointsTests
// {
//     private TaskApiFactory _factory = null!;
//     private HttpClient _client = null!;

//     [SetUp]
//     public void Setup()
//     {
//         _factory = new TaskApiFactory();
//         _client = _factory.CreateClient();
//     }

//     [Test]
// public async Task PostTask_ReturnsBadRequest_WhenValidationFails()
// {
//     // Arrange
//     var dto = new CreateTaskDto("", "", null, "", false);

//     _factory.CreateTaskValidatorMock
//         .Setup(v => v.ValidateAsync(It.IsAny<CreateTaskDto>(), It.IsAny<CancellationToken>()))
//         .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

//     // Act
//     var response = await _client.PostAsJsonAsync("/tasks", dto);

//     // Assert
//     Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
// }

// [Test]
// public async Task PostTask_ReturnsCreated_WhenValidationPasses()
// {
//     // Arrange
//     var dto = new CreateTaskDto("Task A", "Some details", null, "", false);

//     _factory.CreateTaskValidatorMock
//         .Setup(v => v.ValidateAsync(It.IsAny<CreateTaskDto>(), It.IsAny<CancellationToken>()))
//         .ReturnsAsync(new ValidationResult());

//     _factory.TaskServiceMock
//         .Setup(s => s.CreateAsync(It.IsAny<CreateTaskDto>(), It.IsAny<CancellationToken>()))
//         .ReturnsAsync(new BoardTask() {
//             Id = Guid.NewGuid(),
//             Name = dto.Name,
//             Description = dto.Description,
//             Deadline = DateTime.UtcNow,
//             ColumnId = Guid.NewGuid(),
//             IsFavorite = dto.IsFavorite
//         }
//         );

//     // Act
//     var response = await _client.PostAsJsonAsync("/tasks", dto);

//     // Assert
//     Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
// }

// [Test]
// public async Task GetTasks_ReturnsOk()
// {
//     // Arrange
//     _factory.TaskServiceMock
//         .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
//         .ReturnsAsync(new List<BoardTask>
//         {
//             new BoardTask() {
//                 Id = Guid.NewGuid(),
//                 Name = "Task 1",
//                 Description = "Desc",
//                 Deadline = DateTime.UtcNow,
//                 ColumnId = Guid.NewGuid(),
//                 IsFavorite = false
//             }
//         });

//     // Act
//     var response = await _client.GetAsync("/tasks");

//     // Assert
//     Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
//     var tasks = await response.Content.ReadFromJsonAsync<List<BoardTask>>();
//     Assert.That(tasks, Is.Not.Empty);
// }
// }
