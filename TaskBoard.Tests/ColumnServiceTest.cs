using Moq;
using NUnit.Framework;
using TaskBoard.Api.Models;
using TaskBoard.Api.Repositories;
using TaskBoard.Api.Services;

namespace TaskBoard.Tests.TaskBoard.Tests;

[TestFixture]
public class ColumnServiceTests
{
    private Mock<IColumnRepository> _repositoryMock;
    private ColumnService _columnService;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IColumnRepository>();
        _columnService = new ColumnService(_repositoryMock.Object);
    }

    [Test]
    public async Task GetAllAsync()
    {
        // Arrange
        var expected = new List<Column>
        {
            new Column { Id = Guid.Parse("e2aa2883-4285-4b56-8917-08ddeac68d95"), Name = "ToDo" }
        };

        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _columnService.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("ToDo"));
    }

    [Test]
    public async Task AddColumn()
    {
        // Arrange
        var payload = new CreateColumnDto("QA");
        var column = new Column() { Id = Guid.NewGuid(), Name = payload.Name };
        // Act
        var result = await _columnService.AddColumn(payload, CancellationToken.None);
        // Assert
        Assert.That(result.Name, Is.EqualTo("QA"));
    }


}