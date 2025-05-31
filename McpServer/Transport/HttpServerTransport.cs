using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace McpServer.Transport
{
    public static class HttpServerExtension
    {
        public static void StartHttpServer(this IServiceProvider serviceProvider, int port = 5500)
        {
            Task.Run(() => {
                var builder = WebApplication.CreateBuilder();
                
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                });

                // Adiciona referência ao OllamaClient e ApiClient do container de serviços
                builder.Services.AddSingleton(serviceProvider.GetRequiredService<McpServer.Client.OllamaClient>());
                builder.Services.AddSingleton(serviceProvider.GetRequiredService<McpServer.Client.ApiClient>());
                
                var app = builder.Build();
                
                app.UseCors("AllowAll");
                
                // Adiciona o middleware OpenAPI para compatibilidade com Open WebUI
                app.UseOpenApiMiddleware();
                
                // Endpoint para lista de ferramentas disponíveis
                app.MapGet("/api/tools", async (HttpContext context) =>
                {
                    var tools = new List<object>
                    {
                        new
                        {
                            Name = "ObterAsync",
                            Description = "Buscar os livros da livraria, definindo um filtro opcional por titulo",
                            Parameters = new [] 
                            {
                                new { Name = "titulo", Description = "Filtra opcional pelo titulo do livro", Required = false, Type = "string" }
                            }
                        },
                        new
                        {
                            Name = "ObterPorAutor",
                            Description = "Buscar os livros da livraria, definindo um filtro opcional por autor",
                            Parameters = new [] 
                            {
                                new { Name = "autor", Description = "Filtra opcional pelo autor do livro", Required = true, Type = "string" }
                            }
                        },
                        new
                        {
                            Name = "CadastrarAsync",
                            Description = "Criar/Cadastrar um livro",
                            Parameters = new [] 
                            {
                                new { Name = "livro", Description = "dados para criação do livro", Required = true, Type = "object" }
                            }
                        },
                        new
                        {
                            Name = "AtualizarAsync",
                            Description = "Atualizar os dados de um livro",
                            Parameters = new [] 
                            {
                                new { Name = "id", Description = "Código ou identificador do livro", Required = true, Type = "integer" },
                                new { Name = "livro", Description = "Dados para atualização ded um livro", Required = true, Type = "object" }
                            }
                        },
                        new
                        {
                            Name = "VerificarConectividade",
                            Description = "Verifica a conectividade entre os sistemas (MCP Server, Ollama e API de Livros)",
                            Parameters = new object[] { }
                        },
                        new
                        {
                            Name = "ExibirConfiguracao",
                            Description = "Exibe configurações de conexão do MCP Server",
                            Parameters = new object[] { }
                        },
                        new
                        {
                            Name = "ListModels",
                            Description = "Lista os modelos disponíveis no Ollama",
                            Parameters = new object[] { }
                        },
                        new
                        {
                            Name = "GenerateResponse",
                            Description = "Gera uma resposta usando um modelo do Ollama",
                            Parameters = new []
                            {
                                new { Name = "model", Description = "Nome do modelo a ser usado", Required = true, Type = "string" },
                                new { Name = "prompt", Description = "Texto da pergunta ou prompt", Required = true, Type = "string" }
                            }
                        }
                    };
                    
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(tools));
                });
                
                // Endpoint para documento OpenAPI
                app.MapGet("/api/openapi.json", async (HttpContext context) =>
                {
                    var openApiDocument = new
                    {
                        openapi = "3.0.0",
                        info = new
                        {
                            title = "MCP Server API",
                            version = "1.0.0",
                            description = "API para o MCP Server"
                        },
                        paths = new Dictionary<string, object>
                        {
                            ["/api/execute"] = new
                            {
                                post = new
                                {
                                    summary = "Executa uma ferramenta",
                                    requestBody = new
                                    {
                                        required = true,
                                        content = new Dictionary<string, object>
                                        {
                                            ["application/json"] = new
                                            {
                                                schema = new
                                                {
                                                    type = "object",
                                                    properties = new
                                                    {
                                                        tool = new { type = "string", description = "Nome da ferramenta" },
                                                        parameters = new { type = "object", description = "Parâmetros da ferramenta" }
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    responses = new Dictionary<string, object>
                                    {
                                        ["200"] = new
                                        {
                                            description = "Execução bem-sucedida",
                                            content = new Dictionary<string, object>
                                            {
                                                ["application/json"] = new
                                                {
                                                    schema = new
                                                    {
                                                        type = "object",
                                                        properties = new
                                                        {
                                                            result = new { type = "string" }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    };
                    
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(openApiDocument));
                });
                
                // Endpoint para executar ferramentas
                app.MapPost("/api/execute", async (HttpContext context) =>
                {
                    try
                    {
                        // Lê o corpo da requisição
                        using var reader = new StreamReader(context.Request.Body);
                        var requestBody = await reader.ReadToEndAsync();
                        var request = JsonSerializer.Deserialize<ToolExecuteRequest>(requestBody, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (request == null)
                        {
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await context.Response.WriteAsync("Requisição inválida");
                            return;
                        }
                        
                        var result = await ExecuteToolAsync(request, serviceProvider);
                        
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(new { result }));
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsync($"Erro: {ex.Message}");
                    }
                });
                
                // Endpoint para página inicial
                app.MapGet("/", () => "MCP Server HTTP API - Use /api/tools para ver ferramentas disponíveis");

                // Endpoint para modelos Ollama - compatível com Open WebUI
                app.MapGet("/api/ollama/models", async (HttpContext context) =>
                {
                    try
                    {
                        var ollamaClient = serviceProvider.GetRequiredService<McpServer.Client.OllamaClient>();
                        var modelos = await ollamaClient.ListModels();
                        
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(modelos);
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync($"{{\"error\":\"Erro ao listar modelos: {ex.Message}\"}}");
                    }
                });
                
                // Endpoint para geração de texto com Ollama - compatível com Open WebUI
                app.MapPost("/api/ollama/generate", async (HttpContext context) =>
                {
                    try
                    {
                        using var reader = new StreamReader(context.Request.Body);
                        var requestBody = await reader.ReadToEndAsync();
                        var request = JsonSerializer.Deserialize<OllamaGenerateRequest>(requestBody, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        if (request == null || string.IsNullOrEmpty(request.Model) || string.IsNullOrEmpty(request.Prompt))
                        {
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await context.Response.WriteAsync("{{\"error\":\"Parâmetros 'model' e 'prompt' são obrigatórios\"}}");
                            return;
                        }
                        
                        var ollamaClient = serviceProvider.GetRequiredService<McpServer.Client.OllamaClient>();
                        var resposta = await ollamaClient.GenerateResponse(request.Model, request.Prompt);
                        
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync($"{{\"response\":\"{resposta.Replace("\"", "\\\"")}\"}}");
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsync($"{{\"error\":\"Erro ao gerar resposta: {ex.Message}\"}}");
                    }
                });
                
                // Endpoint de status para verificar conectividade com Ollama e API de Livros
                app.MapGet("/api/status", async (HttpContext context) =>
                {
                    try
                    {
                        var ollamaClient = serviceProvider.GetRequiredService<McpServer.Client.OllamaClient>();
                        var apiClient = serviceProvider.GetRequiredService<McpServer.Client.ApiClient>();
                        
                        var status = await McpServer.Tools.WebUITools.VerificarConectividade(apiClient, ollamaClient);
                        
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(status);
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsync($"{{\"error\":\"Erro ao verificar status: {ex.Message}\"}}");
                    }
                });
                
                Console.WriteLine($"Iniciando servidor HTTP na porta {port}");
                app.Run($"http://0.0.0.0:{port}");
            });
        }
        
        private static async Task<string> ExecuteToolAsync(ToolExecuteRequest request, IServiceProvider serviceProvider)
        {
            var ollamaClient = serviceProvider.GetRequiredService<McpServer.Client.OllamaClient>();
            var apiClient = serviceProvider.GetRequiredService<McpServer.Client.ApiClient>();
            
            switch (request.Tool.ToLower())
            {
                case "obterasync":
                    string titulo = null;
                    if (request.Parameters.ContainsKey("titulo"))
                    {
                        titulo = request.Parameters["titulo"]?.ToString();
                    }
                    return await McpServer.Tools.LivrosTools.ObterAsync(apiClient, titulo);
                
                case "obterpor_autor":
                case "obterporautor":
                    if (!request.Parameters.ContainsKey("autor"))
                    {
                        return "Parâmetro 'autor' é obrigatório";
                    }
                    return await McpServer.Tools.LivrosTools.ObterPorAutor(apiClient, request.Parameters["autor"]?.ToString());
                
                case "cadastrarasync":
                    if (!request.Parameters.ContainsKey("livro"))
                    {
                        return "Parâmetro 'livro' é obrigatório";
                    }
                    
                    var jsonLivro = JsonSerializer.Serialize(request.Parameters["livro"]);
                    var livro = JsonSerializer.Deserialize<McpServer.DTOs.LivroRequest>(jsonLivro);
                    
                    return await McpServer.Tools.LivrosTools.CadastrarAsync(apiClient, livro);
                
                case "atualizarasync":
                    if (!request.Parameters.ContainsKey("id") || !request.Parameters.ContainsKey("livro"))
                    {
                        return "Parâmetros 'id' e 'livro' são obrigatórios";
                    }
                    
                    var id = Convert.ToInt32(request.Parameters["id"]);
                    var jsonLivroAtualizar = JsonSerializer.Serialize(request.Parameters["livro"]);
                    var livroAtualizar = JsonSerializer.Deserialize<McpServer.DTOs.LivroRequest>(jsonLivroAtualizar);
                    
                    return await McpServer.Tools.LivrosTools.AtualizarAsync(apiClient, id, livroAtualizar);
                
                case "verificarconectividade":
                    return await McpServer.Tools.WebUITools.VerificarConectividade(apiClient, ollamaClient);
                
                case "exibirconfiguracao":
                    return McpServer.Tools.WebUITools.ExibirConfiguracao();
                
                case "listmodels":
                    return await ollamaClient.ListModels();
                
                case "generateresponse":
                    if (!request.Parameters.ContainsKey("model") || !request.Parameters.ContainsKey("prompt"))
                    {
                        return "Parâmetros 'model' e 'prompt' são obrigatórios";
                    }
                    
                    var model = request.Parameters["model"]?.ToString();
                    var prompt = request.Parameters["prompt"]?.ToString();
                    
                    return await ollamaClient.GenerateResponse(model, prompt);
                
                default:
                    return $"Ferramenta '{request.Tool}' não encontrada";
            }
        }
    }
    
    public class ToolExecuteRequest
    {
        public string Tool { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
    
    public class OllamaGenerateRequest
    {
        public string Model { get; set; }
        public string Prompt { get; set; }
        public bool Stream { get; set; }
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
    }
}
