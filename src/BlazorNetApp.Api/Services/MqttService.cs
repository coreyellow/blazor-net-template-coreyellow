using System.Text;
using System.Text.Json;
using BlazorNetApp.Api.Data;
using BlazorNetApp.Api.Models;
using Microsoft.EntityFrameworkCore;
using MQTTnet;

namespace BlazorNetApp.Api.Services;

public class MqttService : IHostedService, IDisposable
{
    private const string DefaultTopicPrefix = "blazor-net-app";

    private readonly ILogger<MqttService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private IMqttClient? _mqttClient;
    private bool _isConnected = false;
    private readonly JsonSerializerOptions _jsonOptions;
    private string _baseTopicPrefix = DefaultTopicPrefix;

    public MqttService(
        ILogger<MqttService> logger,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var mqttEnabled = _configuration.GetValue<bool>("Mqtt:Enabled");
        if (!mqttEnabled)
        {
            _logger.LogInformation("MQTT is disabled");
            return;
        }
        var broker = _configuration["Mqtt:Broker"] ?? "localhost";
        var port = _configuration.GetValue<int>("Mqtt:Port", 1883);
        _logger.LogInformation("Configuring MQTT client to connect to {Broker}:{Port}", broker, port);
        var clientId = _configuration["Mqtt:ClientId"] ?? $"{DefaultTopicPrefix}-{Guid.NewGuid()}";

        // Extract base topic prefix from configured topic (e.g., "blazor-net-app/#" -> "blazor-net-app")
        var configuredTopic = _configuration["Mqtt:Topic"] ?? $"{DefaultTopicPrefix}/#";
        // Remove MQTT wildcards (# and +) and trailing slashes to get the base prefix
        var topicParts = configuredTopic.Split('/');
        _baseTopicPrefix = topicParts.Length > 0 ? topicParts[0].Trim() : string.Empty;

        if (string.IsNullOrEmpty(_baseTopicPrefix))
        {
            _logger.LogWarning("Invalid MQTT topic configuration, using default prefix: {DefaultPrefix}", DefaultTopicPrefix);
            _baseTopicPrefix = DefaultTopicPrefix;
        }


        _logger.LogInformation("Starting MQTT service, connecting to {Broker}:{Port}", broker, port);

        var mqttFactory = new MqttClientFactory();
        _mqttClient = mqttFactory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(broker, port)
            .WithClientId(clientId)
            .WithCleanSession()
            .Build();

        _mqttClient.ConnectedAsync += OnConnectedAsync;
        _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

        try
        {
            await _mqttClient.ConnectAsync(options, cancellationToken);
            _logger.LogInformation("MQTT client started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MQTT client");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient != null && _mqttClient.IsConnected)
        {
            _logger.LogInformation("Stopping MQTT service");
            await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
        }
    }

    public void Dispose()
    {
        _mqttClient?.Dispose();
    }

    private async Task OnConnectedAsync(MqttClientConnectedEventArgs args)
    {
        _isConnected = true;
        _logger.LogInformation("MQTT client connected");

        // Subscribe to topics
        var topic = _configuration["Mqtt:Topic"] ?? $"{DefaultTopicPrefix}/#";
        if (_mqttClient != null)
        {
            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(topic))
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions);
            _logger.LogInformation("Subscribed to topic: {Topic}", topic);
        }
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        _isConnected = false;
        _logger.LogWarning("MQTT client disconnected");
        return Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;
        var payload = args.ApplicationMessage.ConvertPayloadToString();
        _logger.LogInformation("Received MQTT message on topic {Topic}: {Payload}", topic, payload);

        // Handle command messages
        if (topic.StartsWith($"{_baseTopicPrefix}/command/"))
        {
            await HandleCommandAsync(topic, payload);
        }
    }

    private async Task HandleCommandAsync(string topic, string payload)
    {
        try
        {
            var command = topic.Split('/').LastOrDefault()?.ToLower();
            _logger.LogInformation("Processing command: {Command}", command);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            object? response = null;
            string? correlationId = null;

            // Parse the payload to extract correlation ID if present
            try
            {
                var jsonDoc = JsonDocument.Parse(payload);
                if (jsonDoc.RootElement.TryGetProperty("correlationId", out var corrId))
                {
                    correlationId = corrId.GetString();
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse correlation ID from payload");
            }

            switch (command)
            {
                case "getall":
                    response = await HandleGetAllAsync(context);
                    break;
                case "get":
                    response = await HandleGetByIdAsync(context, payload);
                    break;
                case "create":
                    response = await HandleCreateAsync(context, payload);
                    break;
                case "update":
                case "updatepartial":
                    response = await HandleUpdateAsync(context, payload);
                    break;
                case "delete":
                    response = await HandleDeleteAsync(context, payload);
                    break;
                default:
                    _logger.LogWarning("Unknown command: {Command}", command);
                    response = new { success = false, error = "Unknown command" };
                    break;
            }

            // Publish response
            if (response != null)
            {
                var responseTopic = !string.IsNullOrEmpty(correlationId)
                    ? $"{_baseTopicPrefix}/response/{correlationId}"
                    : $"{_baseTopicPrefix}/response";
                await PublishAsync(responseTopic, response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling command from topic {Topic}", topic);
        }
    }

    private async Task<object> HandleGetAllAsync(ApplicationDbContext context)
    {
        var items = await context.TodoItems.ToListAsync();
        return new { success = true, data = items };
    }

    private async Task<object> HandleGetByIdAsync(ApplicationDbContext context, string payload)
    {
        try
        {
            var request = JsonSerializer.Deserialize<GetByIdRequest>(payload, _jsonOptions);
            if (request?.Id == null)
            {
                return new { success = false, error = "Invalid request: id is required" };
            }

            var item = await context.TodoItems.FindAsync(request.Id);
            if (item == null)
            {
                return new { success = false, error = "Item not found" };
            }

            return new { success = true, data = item };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse get request");
            return new { success = false, error = "Invalid JSON format" };
        }
    }

    private async Task<object> HandleCreateAsync(ApplicationDbContext context, string payload)
    {
        try
        {
            var request = JsonSerializer.Deserialize<CreateTodoRequest>(payload, _jsonOptions);
            if (string.IsNullOrWhiteSpace(request?.Title))
            {
                return new { success = false, error = "Invalid request: title is required" };
            }

            var todoItem = new TodoItem
            {
                Title = request.Title,
                Description = request.Description,
                IsCompleted = request.IsCompleted ?? false,
                CreatedAt = DateTime.UtcNow
            };

            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();

            _logger.LogInformation("Created TODO item via MQTT: {Id}", todoItem.Id);
            return new { success = true, data = todoItem };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse create request");
            return new { success = false, error = "Invalid JSON format" };
        }
    }

    private async Task<object> HandleUpdateAsync(ApplicationDbContext context, string payload)
    {
        try
        {
            var request = JsonSerializer.Deserialize<UpdateTodoRequest>(payload, _jsonOptions);
            if (request?.Id == null)
            {
                return new { success = false, error = "Invalid request: id is required" };
            }

            var existingItem = await context.TodoItems.FindAsync(request.Id);
            if (existingItem == null)
            {
                return new { success = false, error = "Item not found" };
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                existingItem.Title = request.Title;
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                existingItem.Description = request.Description;
            }

            if (request.IsCompleted.HasValue)
            {
                existingItem.IsCompleted = request.IsCompleted.Value;
                if (request.IsCompleted.Value && existingItem.CompletedAt == null)
                {
                    existingItem.CompletedAt = DateTime.UtcNow;
                }
                else if (!request.IsCompleted.Value)
                {
                    existingItem.CompletedAt = null;
                }
            }

            await context.SaveChangesAsync();

            _logger.LogInformation("Updated TODO item via MQTT: {Id}", existingItem.Id);
            return new { success = true, data = existingItem };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse update request");
            return new { success = false, error = "Invalid JSON format" };
        }
    }

    private async Task<object> HandleDeleteAsync(ApplicationDbContext context, string payload)
    {
        try
        {
            var request = JsonSerializer.Deserialize<DeleteRequest>(payload, _jsonOptions);
            if (request?.Id == null)
            {
                return new { success = false, error = "Invalid request: id is required" };
            }

            var item = await context.TodoItems.FindAsync(request.Id);
            if (item == null)
            {
                return new { success = false, error = "Item not found" };
            }

            context.TodoItems.Remove(item);
            await context.SaveChangesAsync();

            _logger.LogInformation("Deleted TODO item via MQTT: {Id}", request.Id);
            return new { success = true, message = "Item deleted successfully" };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse delete request");
            return new { success = false, error = "Invalid JSON format" };
        }
    }

    // Request models for MQTT commands
    private class GetByIdRequest
    {
        public int? Id { get; set; }
        public string? CorrelationId { get; set; }
    }

    private class CreateTodoRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool? IsCompleted { get; set; }
        public string? CorrelationId { get; set; }
    }

    private class UpdateTodoRequest
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool? IsCompleted { get; set; }
        public string? CorrelationId { get; set; }
    }

    private class DeleteRequest
    {
        public int? Id { get; set; }
        public string? CorrelationId { get; set; }
    }

    public async Task PublishAsync(string topic, object payload)
    {
        if (_mqttClient == null || !_isConnected)
        {
            _logger.LogWarning("Cannot publish: MQTT client is not connected");
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(payload);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(json)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            await _mqttClient.PublishAsync(message);
            _logger.LogInformation("Published message to topic {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish MQTT message");
        }
    }

    public bool IsConnected => _isConnected;

    public string BaseTopicPrefix => _baseTopicPrefix;
}
