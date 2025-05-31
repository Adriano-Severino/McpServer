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

builder.Services.AddHttpClient<OllamaClient>(client =>
{
    // Possibilita acessar o Ollama tanto localmente quanto via Docker
    var ollamaAddress = Environment.GetEnvironmentVariable("OLLAMA_BASE_URL") 
        ?? "http://host.docker.internal:11434";
    client.BaseAddress = new Uri(ollamaAddress);
});

builder.Services.AddHttpClient<ApiClient>(client =>
{
    var baseAddress = Environment.GetEnvironmentVariable("API_BASE_ADDRESS");
    if (!string.IsNullOrEmpty(baseAddress))
        client.BaseAddress = new Uri(baseAddress);
    else
    {
        // Tenta primeiro com HTTPS na porta 7294
        try
        {
            var testClient = new HttpClient();
            var response = testClient.GetAsync("https://mcpserver-livros-api-1/api/v1/Livros").Result;
            client.BaseAddress = new Uri("https://mcpserver-livros-api-1:7294/api/");
            Console.WriteLine("Conectado à API Livros via HTTPS");
        }
        catch
        {
            // Fallback para HTTP na porta 5000 ou 5001
            client.BaseAddress = new Uri("http://mcpserver-livros-api-1:5000/api/");
            Console.WriteLine("Conectado à API Livros via HTTP");
        }
    }
});

var app = builder.Build();

// Inicia o servidor HTTP na porta 5500
var serviceProvider = app.Services;
McpServer.Transport.HttpServerExtension.StartHttpServer(serviceProvider);
Console.WriteLine("------------------------------------------------------------------------------");
Console.WriteLine("Servidor HTTP iniciado na porta 5500");
Console.WriteLine("URL para conectar ao Open WebUI: http://host.docker.internal:5500/api/openapi.json");
Console.WriteLine("------------------------------------------------------------------------------");

await app.RunAsync();