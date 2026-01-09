using BlazorNetApp.Api.Data;
using BlazorNetApp.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

// Configure paths to work when app is run as a global tool
// When published/packaged, wwwroot and content are in AppContext.BaseDirectory
var webRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
    WebRootPath = Directory.Exists(webRootPath) ? webRootPath : null
};

var builder = WebApplication.CreateBuilder(options);

// Add configuration from current directory (user override)
// This allows users to override packaged configuration by placing appsettings files
// in the directory from which they run the application. Since configuration sources
// are loaded in order and later sources override earlier ones, these files will take
// precedence over the packaged configuration files from AppContext.BaseDirectory.
var env = builder.Environment;
builder.Configuration.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: true, reloadOnChange: true)
                     .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{env.EnvironmentName}.json"), optional: true, reloadOnChange: true);

// Enable systemd integration for proper service lifecycle management
builder.Host.UseSystemd();

// Add services to the container
builder.Services.AddControllers().AddNewtonsoftJson();

// Configure Entity Framework with SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// If no connection string is configured, determine an appropriate default based on the environment and platform
if (string.IsNullOrEmpty(connectionString))
{
    // For Production on Linux/Unix systems running as a systemd service, use /var/lib
    if (builder.Environment.IsProduction() && (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD()))
    {
        var dataDir = "/var/lib/blazor-net-app";
        // Only use /var/lib if the directory exists or can be created (systemd service scenario)
        try
        {
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            connectionString = $"Data Source={Path.Combine(dataDir, "blazor-net-app.db")}";
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            // If we can't create the directory (not running as service), fall back to current directory
            // This is expected when running as a regular user without permissions to /var/lib
            var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Program");
            logger.LogInformation("Unable to create database directory at {DataDir}, using current directory instead. Reason: {Reason}",
                dataDir, ex.Message);
            connectionString = "Data Source=blazor-net-app.db";
        }
    }
    else
    {
        // For Development, Windows, or when /var/lib is not accessible, use current directory
        connectionString = "Data Source=blazor-net-app.db";
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Add MQTT service
builder.Services.AddSingleton<MqttService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<MqttService>());

// Add WebSocket service for real-time TODO updates
builder.Services.AddSingleton<TodoWebSocketService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Net App API",
        Version = "v1",
        Description = "A .NET REST API template with MQTT support, SQLite database, and Blazor UI",
        Contact = new()
        {
            Name = "MLM Devs",
            Url = new Uri("https://github.com/mlmdevs/mlmdevs-net-template")
        }
    });

    // Enable XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Blazor Server for the web UI
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpClient for API calls from Blazor components
builder.Services.AddHttpClient("ServerAPI")
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    // In development, bypass SSL certificate validation for self-signed certificates
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});

// Configure the scoped HttpClient with BaseAddress from NavigationManager
// This is done at the scoped level to avoid resolving scoped services from root provider
builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = httpClientFactory.CreateClient("ServerAPI");

    // Set BaseAddress from NavigationManager (available in scoped context)
    var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    client.BaseAddress = new Uri(navigationManager.BaseUri);

    return client;
});

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Net App API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseWebSockets();

app.UseStaticFiles();

app.UseRouting();
app.UseCors();
app.UseAuthorization();
app.UseAntiforgery();

// WebSocket endpoint for TODO updates
app.Map("/ws/todos", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();
        var wsService = context.RequestServices.GetRequiredService<TodoWebSocketService>();
        await wsService.HandleWebSocketAsync(webSocket, connectionId);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.MapControllers();

// Map Blazor components
app.MapRazorComponents<BlazorNetApp.Api.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
