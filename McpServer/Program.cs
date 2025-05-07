using McpServer.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;

var builder = Host.CreateApplicationBuilder();

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Debug;
});

builder.Configuration.AddEnvironmentVariables();

var serverinfo = new Implementation { Name = "DotNetMCPServer" , Version = "1.0.0" };

builder.Services
    .AddMcpServer(mcp =>
    {
        mcp.ServerInfo = serverinfo;
    })
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddHttpClient<ApiClient>(client =>
{
    var baseAddress = Environment.GetEnvironmentVariable("API_BASE_ADDRESS");
    if (!string.IsNullOrEmpty(baseAddress))
        client.BaseAddress = new Uri(baseAddress);
    else;
    client.BaseAddress = new Uri("https://localhost:7294/api/");
});

var app = builder.Build();
await app.RunAsync();