using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TaskBoard.Api.Models;
using TaskBoard.Api.Repositories;

namespace TaskBoard.Tests.Repositories
{
    [TestFixture]
    public class EfColumnRepositoryTests
    {
        private TaskBoardDbContext _dbContext = null!;
        private EfColumnRepository _repository = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TaskBoardDbContext>()
                .UseInMemoryDatabase(databaseName: $"TaskBoardTestDb_{System.Guid.NewGuid()}")
                .Options;

            _dbContext = new TaskBoardDbContext(options);
            _repository = new EfColumnRepository(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddColumn()
        {
            // Arrange
            var column = new Column { Id = Guid.NewGuid(), Name = "To Do" };

            // Act
            await _repository.AddAsync(column, CancellationToken.None);

            // Assert
            var result = await _dbContext.Columns.FirstOrDefaultAsync(c => c.Id == column.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("To Do"));
        }

        [Test]
        public async Task GetAsync_ShouldReturnColumnWithTasks()
        {
            // Arrange
            var column = new Column
            {
                Id = Guid.NewGuid(),
                Name = "In Progress",
                Tasks = new List<BoardTask>
                {
                    new BoardTask { Id = Guid.NewGuid(), Name = "Test Task" }
                }
            };

            _dbContext.Columns.Add(column);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(column.Id.ToString(), CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Tasks.Count, Is.EqualTo(1));
            Assert.That(result.Tasks[0].Name, Is.EqualTo("Test Task"));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllColumns()
        {
            // Arrange
            var col1 = new Column { Id = Guid.NewGuid(), Name = "Backlog" };
            var col2 = new Column { Id = Guid.NewGuid(), Name = "Done" };

            _dbContext.Columns.AddRange(col1, col2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(c => c.Name == "Backlog"), Is.True);
            Assert.That(result.Any(c => c.Name == "Done"), Is.True);
        }
    }
}
