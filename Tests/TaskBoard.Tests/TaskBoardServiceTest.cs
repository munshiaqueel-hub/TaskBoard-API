using Moq;
using NUnit.Framework;
using TaskBoard.Api.Models;
using TaskBoard.Api.Repositories;
using TaskBoard.Api.Services;

namespace TaskBoard.Tests.Services
{
    [TestFixture]
    public class TaskServiceTests
    {
        private Mock<ITaskRepository> _taskRepo;
        private Mock<IColumnRepository> _columnRepo;
        private Mock<IImageStorage> _imageStorage;
        private TaskService _service;

        [SetUp]
        public void Setup()
        {
            _taskRepo = new Mock<ITaskRepository>();
            _columnRepo = new Mock<IColumnRepository>();
            _imageStorage = new Mock<IImageStorage>();

            _service = new TaskService(_taskRepo.Object, _columnRepo.Object, _imageStorage.Object);
        }

        // ------------------- POSITIVE TESTS -------------------

        [Test]
        public async Task CreateAsync_ShouldCreateTask_WhenValid()
        {
            var column = new Column { Id = Guid.NewGuid(), Name = "Test" };
            _columnRepo.Setup(r => r.GetAsync(column.Id.ToString(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(column);

            var dto = new CreateTaskDto("My Task", null, null, column.Id.ToString(), false);

            var result = await _service.CreateAsync(dto, CancellationToken.None);

            Assert.That(result.Name, Is.EqualTo("My Task"));
            _taskRepo.Verify(r => r.AddAsync(It.IsAny<BoardTask>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void CreateAsync_ShouldThrow_WhenNameMissing()
        {
            var dto = new CreateTaskDto("", null, null, Guid.NewGuid().ToString(), false);

            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateAsync(dto, CancellationToken.None));
        }

        [Test]
        public async Task GetAsync_ShouldCallRepo()
        {
            var id = new TaskId(Guid.NewGuid());
            var task = new BoardTask { Id = id.Value };
            _taskRepo.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);

            var result = await _service.GetAsync(id, CancellationToken.None);

            Assert.That(result?.Id, Is.EqualTo(id.Value));
        }

        [Test]
        public async Task EditAsync_ShouldUpdateTask()
        {
            var id = new TaskId(Guid.NewGuid());
            var task = new BoardTask { Id = id.Value, Name = "Old" };

            _taskRepo.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);

            var dto = new EditTaskDto("New Name", null, null, false);

            await _service.EditAsync(id, dto, CancellationToken.None);

            Assert.That(task.Name, Is.EqualTo("New Name"));
            _taskRepo.Verify(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_ShouldCallRepo()
        {
            var id = new TaskId(Guid.NewGuid());
            await _service.DeleteAsync(id, CancellationToken.None);

            _taskRepo.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task MoveAsync_ShouldUpdateColumn()
        {
            var id = new TaskId(Guid.NewGuid());
            var task = new BoardTask { Id = id.Value, ColumnId = Guid.NewGuid() };
            var newColumn = new Column { Id = Guid.NewGuid() };

            _taskRepo.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);
            _columnRepo.Setup(r => r.GetAsync(newColumn.Id.ToString(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newColumn);

            await _service.MoveAsync(id, newColumn.Id.ToString(), CancellationToken.None);

            Assert.That(task.ColumnId, Is.EqualTo(newColumn.Id));
            _taskRepo.Verify(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task AttachImageAsync_ShouldUploadAndInsertImage()
        {
            var taskId = new TaskId(Guid.NewGuid());

            var fileMock = new Mock<IFormFile>();
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("filecontent"));
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns("test.png");
            fileMock.Setup(f => f.ContentType).Returns("image/png");

            var files = new FormFileCollection { fileMock.Object };

            _imageStorage.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), "image/png", It.IsAny<CancellationToken>()))
                .ReturnsAsync("http://test.com/test.png");

            _taskRepo.Setup(r => r.InsertImages(It.IsAny<TaskImage>(), It.IsAny<CancellationToken>()));
            _taskRepo.Setup(r => r.SaveChanges(It.IsAny<CancellationToken>()));

            var urls = await _service.AttachImageAsync(taskId, files, CancellationToken.None);

            Assert.That(urls.Count, Is.EqualTo(1));
            Assert.That(urls[0], Is.EqualTo("http://test.com/test.png"));

            _taskRepo.Verify(r => r.InsertImages(It.IsAny<TaskImage>(), It.IsAny<CancellationToken>()), Times.Once);
            _taskRepo.Verify(r => r.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        }

        // ------------------- NEGATIVE TESTS -------------------

        [Test]
        public void CreateAsync_ShouldThrow_WhenColumnNotFound()
        {
            var dto = new CreateTaskDto("Task", null, null, Guid.NewGuid().ToString(), false);

            _columnRepo.Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Column?)null);

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateAsync(dto, CancellationToken.None));
        }

        [Test]
        public void EditAsync_ShouldThrow_WhenTaskNotFound()
        {
            var id = new TaskId(Guid.NewGuid());
            _taskRepo.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BoardTask?)null);

            var dto = new EditTaskDto("Name", null, null, false);

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.EditAsync(id, dto, CancellationToken.None));
        }

        [Test]
        public void MoveAsync_ShouldThrow_WhenTaskNotFound()
        {
            var id = new TaskId(Guid.NewGuid());
            _taskRepo.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BoardTask?)null);

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.MoveAsync(id, Guid.NewGuid().ToString(), CancellationToken.None));
        }

        [Test]
        public void MoveAsync_ShouldThrow_WhenColumnNotFound()
        {
            var id = new TaskId(Guid.NewGuid());
            var task = new BoardTask { Id = id.Value, ColumnId = Guid.NewGuid() };

            _taskRepo.Setup(r => r.GetAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);
            _columnRepo.Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Column?)null);

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.MoveAsync(id, Guid.NewGuid().ToString(), CancellationToken.None));
        }

        [Test]
        public void AttachImageAsync_ShouldThrow_WhenNoFilesProvided()
        {
            var taskId = new TaskId(Guid.NewGuid());
            var files = new FormFileCollection(); // empty

            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AttachImageAsync(taskId, files, CancellationToken.None));
        }
    }
}
