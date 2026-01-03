using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using BlazorNetApp.Api.Models;
using Xunit;

namespace BlazorNetApp.IntegrationTests.Services;

public class TodoWebSocketTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public TodoWebSocketTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task WebSocket_ReceivesCreatedNotification_WhenTodoIsCreatedAsync()
    {
        // Arrange
        var wsClient = _factory.Server.CreateWebSocketClient();
        var wsUri = new Uri(_factory.Server.BaseAddress, "/ws/todos");
        WebSocket? webSocket = null;

        try
        {
            webSocket = await wsClient.ConnectAsync(wsUri, CancellationToken.None);
            Assert.Equal(WebSocketState.Open, webSocket.State);

            // Start listening for messages
            var receiveTask = ReceiveWebSocketMessageAsync(webSocket);

            // Give WebSocket time to establish
            await Task.Delay(100);

            // Act - Create a TODO item via API
            var newItem = new TodoItem
            {
                Title = "WebSocket Test TODO",
                Description = "Testing WebSocket notifications",
                IsCompleted = false
            };

            var response = await _httpClient.PostAsJsonAsync("/api/todoitems", newItem);
            response.EnsureSuccessStatusCode();
            var createdItem = await response.Content.ReadFromJsonAsync<TodoItem>();

            // Assert - Should receive WebSocket notification
            var message = await receiveTask;
            Assert.NotNull(message);

            var messageDoc = JsonDocument.Parse(message);
            var action = messageDoc.RootElement.GetProperty("action").GetString();
            Assert.Equal("created", action);

            var data = messageDoc.RootElement.GetProperty("data");
            var wsItemId = data.GetProperty("Id").GetInt32();
            Assert.Equal(createdItem!.Id, wsItemId);
        }
        finally
        {
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
            }
            webSocket?.Dispose();
        }
    }

    [Fact]
    public async Task WebSocket_ReceivesUpdatedNotification_WhenTodoIsUpdatedAsync()
    {
        // Arrange - Create a TODO first
        var newItem = new TodoItem
        {
            Title = "Test TODO for Update",
            Description = "Original",
            IsCompleted = false
        };

        var createResponse = await _httpClient.PostAsJsonAsync("/api/todoitems", newItem);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        var wsClient = _factory.Server.CreateWebSocketClient();
        var wsUri = new Uri(_factory.Server.BaseAddress, "/ws/todos");
        WebSocket? webSocket = null;

        try
        {
            webSocket = await wsClient.ConnectAsync(wsUri, CancellationToken.None);
            Assert.Equal(WebSocketState.Open, webSocket.State);

            var receiveTask = ReceiveWebSocketMessageAsync(webSocket);
            await Task.Delay(100);

            // Act - Update the TODO item
            createdItem!.Title = "Updated Title";
            createdItem.IsCompleted = true;

            var response = await _httpClient.PutAsJsonAsync($"/api/todoitems/{createdItem.Id}", createdItem);
            response.EnsureSuccessStatusCode();

            // Assert
            var message = await receiveTask;
            Assert.NotNull(message);

            var messageDoc = JsonDocument.Parse(message);
            var action = messageDoc.RootElement.GetProperty("action").GetString();
            Assert.Equal("updated", action);

            var data = messageDoc.RootElement.GetProperty("data");
            var wsItemId = data.GetProperty("Id").GetInt32();
            Assert.Equal(createdItem.Id, wsItemId);
        }
        finally
        {
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
            }
            webSocket?.Dispose();
        }
    }

    [Fact]
    public async Task WebSocket_ReceivesDeletedNotification_WhenTodoIsDeletedAsync()
    {
        // Arrange - Create a TODO first
        var newItem = new TodoItem
        {
            Title = "Test TODO for Delete",
            Description = "To be deleted",
            IsCompleted = false
        };

        var createResponse = await _httpClient.PostAsJsonAsync("/api/todoitems", newItem);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        var wsClient = _factory.Server.CreateWebSocketClient();
        var wsUri = new Uri(_factory.Server.BaseAddress, "/ws/todos");
        WebSocket? webSocket = null;

        try
        {
            webSocket = await wsClient.ConnectAsync(wsUri, CancellationToken.None);
            Assert.Equal(WebSocketState.Open, webSocket.State);

            var receiveTask = ReceiveWebSocketMessageAsync(webSocket);
            await Task.Delay(100);

            // Act - Delete the TODO item
            var response = await _httpClient.DeleteAsync($"/api/todoitems/{createdItem!.Id}");
            response.EnsureSuccessStatusCode();

            // Assert
            var message = await receiveTask;
            Assert.NotNull(message);

            var messageDoc = JsonDocument.Parse(message);
            var action = messageDoc.RootElement.GetProperty("action").GetString();
            Assert.Equal("deleted", action);

            var data = messageDoc.RootElement.GetProperty("data");
            var wsItemId = data.GetProperty("id").GetInt32();
            Assert.Equal(createdItem.Id, wsItemId);
        }
        finally
        {
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
            }
            webSocket?.Dispose();
        }
    }

    private async Task<string> ReceiveWebSocketMessageAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        return Encoding.UTF8.GetString(buffer, 0, result.Count);
    }
}
