using BlazorNetApp.Api.Data;
using BlazorNetApp.Api.Models;
using BlazorNetApp.Api.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorNetApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TodoItemsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly MqttService _mqttService;
    private readonly TodoWebSocketService _webSocketService;
    private readonly ILogger<TodoItemsController> _logger;

    public TodoItemsController(
        ApplicationDbContext context,
        MqttService mqttService,
        TodoWebSocketService webSocketService,
        ILogger<TodoItemsController> logger)
    {
        _context = context;
        _mqttService = mqttService;
        _webSocketService = webSocketService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all TODO items
    /// </summary>
    /// <returns>A list of TODO items</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TodoItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItemsAsync()
    {
        var items = await _context.TodoItems.ToListAsync();
        return Ok(items);
    }

    /// <summary>
    /// Gets a specific TODO item by ID
    /// </summary>
    /// <param name="id">The ID of the TODO item</param>
    /// <returns>The TODO item</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TodoItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItem>> GetTodoItemAsync(int id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);

        if (todoItem == null)
        {
            return NotFound();
        }

        return Ok(todoItem);
    }

    /// <summary>
    /// Creates a new TODO item
    /// </summary>
    /// <param name="todoItem">The TODO item to create</param>
    /// <returns>The created TODO item</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TodoItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoItem>> CreateTodoItemAsync(TodoItem todoItem)
    {
        todoItem.CreatedAt = DateTime.UtcNow;
        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();

        // Publish MQTT message
        await _mqttService.PublishAsync($"{_mqttService.BaseTopicPrefix}/todo/created", new
        {
            id = todoItem.Id,
            title = todoItem.Title,
            timestamp = DateTime.UtcNow
        });

        // Broadcast via WebSocket
        await _webSocketService.BroadcastTodoChangeAsync("created", todoItem);

        return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
    }

    /// <summary>
    /// Updates an existing TODO item
    /// </summary>
    /// <param name="id">The ID of the TODO item to update</param>
    /// <param name="todoItem">The updated TODO item data</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTodoItemAsync(int id, TodoItem todoItem)
    {
        if (id != todoItem.Id)
        {
            return BadRequest();
        }

        var existingItem = await _context.TodoItems.FindAsync(id);
        if (existingItem == null)
        {
            return NotFound();
        }

        existingItem.Title = todoItem.Title;
        existingItem.Description = todoItem.Description;
        existingItem.IsCompleted = todoItem.IsCompleted;

        if (todoItem.IsCompleted && !existingItem.IsCompleted)
        {
            existingItem.CompletedAt = DateTime.UtcNow;
        }
        else if (!todoItem.IsCompleted)
        {
            existingItem.CompletedAt = null;
        }

        await _context.SaveChangesAsync();

        // Publish MQTT message
        await _mqttService.PublishAsync($"{_mqttService.BaseTopicPrefix}/todo/updated", new
        {
            id = todoItem.Id,
            title = todoItem.Title,
            isCompleted = todoItem.IsCompleted,
            timestamp = DateTime.UtcNow
        });

        // Broadcast via WebSocket
        await _webSocketService.BroadcastTodoChangeAsync("updated", existingItem);

        return NoContent();
    }

    /// <summary>
    /// Updates the fields of an existing TODO item
    /// </summary>
    /// <param name="id">The ID of the TODO item to update</param>
    /// <param name="patchDoc">The JSON Patch document with the changes</param>
    /// <returns>No content on success</returns>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePartialTodoItemAsync(int id, [FromBody] JsonPatchDocument<TodoItem> patchDoc)
    {
        if (patchDoc == null)
        {
            return BadRequest();
        }

        var existingItem = await _context.TodoItems.FindAsync(id);
        if (existingItem == null)
        {
            return NotFound();
        }

        patchDoc.ApplyTo(existingItem, jsonPatchError =>
            {
                var key = jsonPatchError.AffectedObject.GetType().Name;
                ModelState.AddModelError(key, jsonPatchError.ErrorMessage);
            }
        );
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _context.SaveChangesAsync();

        // Publish MQTT message
        await _mqttService.PublishAsync($"{_mqttService.BaseTopicPrefix}/todo/updatedpartial", new
        {
            id = existingItem.Id,
            title = existingItem.Title,
            isCompleted = existingItem.IsCompleted,
            timestamp = DateTime.UtcNow
        });

        // Broadcast via WebSocket
        await _webSocketService.BroadcastTodoChangeAsync("updatedpartial", existingItem);

        return NoContent();
    }

    /// <summary>
    /// Deletes a TODO item
    /// </summary>
    /// <param name="id">The ID of the TODO item to delete</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTodoItemAsync(int id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem == null)
        {
            return NotFound();
        }

        _context.TodoItems.Remove(todoItem);
        await _context.SaveChangesAsync();

        // Publish MQTT message
        await _mqttService.PublishAsync($"{_mqttService.BaseTopicPrefix}/todo/deleted", new
        {
            id = todoItem.Id,
            timestamp = DateTime.UtcNow
        });

        // Broadcast via WebSocket
        await _webSocketService.BroadcastTodoChangeAsync("deleted", new { id = todoItem.Id });

        return NoContent();
    }
}
