using BlazorNetApp.Api.Models;
using FluentAssertions;
using Xunit;

namespace BlazorNetApp.Tests.Models;

public class TodoItemTests
{
    [Fact]
    public void TodoItem_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var todoItem = new TodoItem();

        // Assert
        todoItem.Id.Should().Be(0);
        todoItem.Title.Should().BeEmpty();
        todoItem.Description.Should().BeNull();
        todoItem.IsCompleted.Should().BeFalse();
        todoItem.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void TodoItem_ShouldSetProperties()
    {
        // Arrange
        var title = "Test TODO";
        var description = "Test description";
        var createdAt = DateTime.UtcNow;

        // Act
        var todoItem = new TodoItem
        {
            Id = 1,
            Title = title,
            Description = description,
            IsCompleted = true,
            CreatedAt = createdAt,
            CompletedAt = createdAt.AddHours(1)
        };

        // Assert
        todoItem.Id.Should().Be(1);
        todoItem.Title.Should().Be(title);
        todoItem.Description.Should().Be(description);
        todoItem.IsCompleted.Should().BeTrue();
        todoItem.CreatedAt.Should().Be(createdAt);
        todoItem.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void TodoItem_CanBeMarkedAsCompleted()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Title = "Test",
            IsCompleted = false
        };

        // Act
        todoItem.IsCompleted = true;
        todoItem.CompletedAt = DateTime.UtcNow;

        // Assert
        todoItem.IsCompleted.Should().BeTrue();
        todoItem.CompletedAt.Should().NotBeNull();
    }
}
