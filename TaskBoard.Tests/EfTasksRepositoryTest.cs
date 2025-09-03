using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TaskBoard.Api.Models;
using TaskBoard.Api.Repositories;

namespace TaskBoard.Tests.Repositories
{
    [TestFixture]
    public class EfTaskRepositoryTests
    {
        private TaskBoardDbContext _db = null!;
        private EfTaskRepository _repo = null!;
        private CancellationToken _ct;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TaskBoardDbContext>()
                .UseInMemoryDatabase(databaseName: $"TaskBoardDb_{Guid.NewGuid()}")
                .Options;

            _db = new TaskBoardDbContext(options);
            _db.Database.EnsureCreated();
            _repo = new EfTaskRepository(_db);
            _ct = CancellationToken.None;
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddTask()
        {
            var task = new BoardTask
            {
                Id = Guid.NewGuid(),
                Name = "Task 1",
                Description = "Test",
                ColumnId = Guid.NewGuid(),
                CreatedOn = DateTimeOffset.UtcNow
            };

            await _repo.AddAsync(task, _ct);

            var found = await _repo.GetAsync(new TaskId(task.Id), _ct);
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Task 1"));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateTask()
        {
            var task = new BoardTask
            {
                Id = Guid.NewGuid(),
                Name = "Old Name",
                Description = "Old",
                ColumnId = Guid.NewGuid(),
                CreatedOn = DateTimeOffset.UtcNow
            };
            await _repo.AddAsync(task, _ct);

            task.Name = "New Name";
            await _repo.UpdateAsync(task, _ct);

            var found = await _repo.GetAsync(new TaskId(task.Id), _ct);
            Assert.That(found!.Name, Is.EqualTo("New Name"));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveTask()
        {
            var task = new BoardTask
            {
                Id = Guid.NewGuid(),
                Name = "Delete Me",
                ColumnId = Guid.NewGuid(),
                CreatedOn = DateTimeOffset.UtcNow
            };
            await _repo.AddAsync(task, _ct);

            await _repo.DeleteAsync(new TaskId(task.Id), _ct);

            var found = await _repo.GetAsync(new TaskId(task.Id), _ct);
            Assert.That(found, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllTasks()
        {
            var t1 = new BoardTask { Id = Guid.NewGuid(), Name = "T1", ColumnId = Guid.NewGuid(), CreatedOn = DateTimeOffset.UtcNow };
            var t2 = new BoardTask { Id = Guid.NewGuid(), Name = "T2", ColumnId = Guid.NewGuid(), CreatedOn = DateTimeOffset.UtcNow };

            await _repo.AddAsync(t1, _ct);
            await _repo.AddAsync(t2, _ct);

            var tasks = await _repo.GetAllAsync(_ct);
            Assert.That(tasks.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetByColumnAsync_ShouldReturnFilteredTasks()
        {
            var columnId = Guid.NewGuid();
            var t1 = new BoardTask { Id = Guid.NewGuid(), Name = "In column", ColumnId = columnId, CreatedOn = DateTimeOffset.UtcNow };
            var t2 = new BoardTask { Id = Guid.NewGuid(), Name = "Other column", ColumnId = Guid.NewGuid(), CreatedOn = DateTimeOffset.UtcNow };

            await _repo.AddAsync(t1, _ct);
            await _repo.AddAsync(t2, _ct);

            var tasks = await _repo.GetByColumnAsync(columnId, _ct);
            Assert.That(tasks.Count, Is.EqualTo(1));
            Assert.That(tasks.First().Name, Is.EqualTo("In column"));
        }
    }
}
