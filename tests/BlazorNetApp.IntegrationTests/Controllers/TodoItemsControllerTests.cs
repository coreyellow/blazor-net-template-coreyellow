using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BlazorNetApp.Api.Models;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BlazorNetApp.IntegrationTests.Controllers;

public class TodoItemsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public TodoItemsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    [Fact]
    public async Task GetTodoItems_ReturnsSuccessAndSeededItemsAsync()
    {
        // Act
        var response = await _client.GetAsync("/api/todoitems");

        // Assert
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<List<TodoItem>>(_jsonOptions);

        Assert.NotNull(items);
        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task CreateTodoItem_ReturnsCreatedItemAsync()
    {
        // Arrange
        var newItem = new TodoItem
        {
            Title = "Test TODO",
            Description = "Test description",
            IsCompleted = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todoitems", newItem);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdItem = await response.Content.ReadFromJsonAsync<TodoItem>(_jsonOptions);
        Assert.NotNull(createdItem);
        Assert.Equal(newItem.Title, createdItem.Title);
        Assert.Equal(newItem.Description, createdItem.Description);
        Assert.True(createdItem.Id > 0);
    }

    [Fact]
    public async Task GetTodoItem_WithValidId_ReturnsItemAsync()
    {
        // Arrange - Create an item first
        var newItem = new TodoItem
        {
            Title = "Test TODO for Get",
            Description = "Test description",
            IsCompleted = false
        };
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", newItem);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>(_jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/todoitems/{createdItem!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var item = await response.Content.ReadFromJsonAsync<TodoItem>(_jsonOptions);
        Assert.NotNull(item);
        Assert.Equal(createdItem.Id, item.Id);
        Assert.Equal(newItem.Title, item.Title);
    }

    [Fact]
    public async Task GetTodoItem_WithInvalidId_ReturnsNotFoundAsync()
    {
        // Act
        var response = await _client.GetAsync("/api/todoitems/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTodoItem_WithValidData_ReturnsNoContentAsync()
    {
        // Arrange - Create an item first
        var newItem = new TodoItem
        {
            Title = "Test TODO for Update",
            Description = "Original description",
            IsCompleted = false
        };
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", newItem);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>(_jsonOptions);

        // Modify the item
        createdItem!.Title = "Updated Title";
        createdItem.IsCompleted = true;

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todoitems/{createdItem.Id}", createdItem);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/todoitems/{createdItem.Id}");
        var updatedItem = await getResponse.Content.ReadFromJsonAsync<TodoItem>(_jsonOptions);
        Assert.Equal("Updated Title", updatedItem!.Title);
        Assert.True(updatedItem.IsCompleted);
    }

    [Fact]
    public async Task UpdatePartialTodoItem_WithValidData_ReturnsNoContentAsync()
    {
        // Arrange - Create an item first
        var newItem = new TodoItem
        {
            Title = "Test TODO for UpdatePartial",
            Description = "Original description",
            IsCompleted = false
        };
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", newItem);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>(_jsonOptions);

        // Create patch document
        var patchDoc = new JsonPatchDocument<TodoItem>();
        patchDoc.Replace(t => t.Title, "Updated Title");
        patchDoc.Replace(t => t.IsCompleted, true);

        // Act
        var json = JsonConvert.SerializeObject(patchDoc);

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/todoitems/{createdItem!.Id}")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json-patch+json")
        };

        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/todoitems/{createdItem.Id}");
        var updatedItem = await getResponse.Content.ReadFromJsonAsync<TodoItem>(_jsonOptions);
        Assert.Equal("Updated Title", updatedItem!.Title);
        Assert.True(updatedItem.IsCompleted);
    }

    [Fact]
    public async Task DeleteTodoItem_WithValidId_ReturnsNoContentAsync()
    {
        // Arrange - Create an item first
        var newItem = new TodoItem
        {
            Title = "Test TODO for Delete",
            Description = "To be deleted",
            IsCompleted = false
        };
        var createResponse = await _client.PostAsJsonAsync("/api/todoitems", newItem);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>(_jsonOptions);

        // Act
        var response = await _client.DeleteAsync($"/api/todoitems/{createdItem!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/todoitems/{createdItem.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteTodoItem_WithInvalidId_ReturnsNotFoundAsync()
    {
        // Act
        var response = await _client.DeleteAsync("/api/todoitems/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
