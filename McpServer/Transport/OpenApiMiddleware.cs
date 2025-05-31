using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace McpServer.Transport
{
    public class OpenApiMiddleware
    {
        private readonly RequestDelegate _next;

        public OpenApiMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Configura o CORS para o Open WebUI
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");

            // Adiciona cabeçalho OpenAPI para compatibilidade com ferramentas como o Open WebUI
            context.Response.Headers.Append("X-OpenAPI-Version", "3.0.0");

            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = 200;
                return;
            }

            // Middleware especial para rota OpenAPI
            if (context.Request.Path.Value != null && 
                (context.Request.Path.Value.Equals("/openapi.json", StringComparison.OrdinalIgnoreCase) ||
                 context.Request.Path.Value.Equals("/api/openapi.json", StringComparison.OrdinalIgnoreCase)))
            {
                await HandleOpenApiRequest(context);
                return;
            }

            await _next(context);
        }

        private async Task HandleOpenApiRequest(HttpContext context)
        {
            var openApiDocument = new
            {
                openapi = "3.0.0",
                info = new
                {
                    title = "MCP Server API",
                    version = "1.0.0",
                    description = "API para o MCP Server integrado com Ollama e API de Livros"
                },
                servers = new[]
                {
                    new { url = "/" }
                },
                paths = GenerateOpenApiPaths()
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(openApiDocument, 
                new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
        }

        private static object GenerateOpenApiPaths()
        {
            // Definição dos endpoints da API
            return new Dictionary<string, object>
            {
                ["/api/livros"] = new
                {
                    get = new
                    {
                        summary = "Obter livros",
                        operationId = "ObterLivros",
                        parameters = new[]
                        {
                            new
                            {
                                name = "titulo",
                                @in = "query",
                                description = "Filtro opcional por título",
                                required = false,
                                schema = new { type = "string" }
                            }
                        },
                        responses = DefaultResponses()
                    }
                },
                ["/api/livros/autor/{autor}"] = new
                {
                    get = new
                    {
                        summary = "Obter livros por autor",
                        operationId = "ObterLivrosPorAutor",
                        parameters = new[]
                        {
                            new
                            {
                                name = "autor",
                                @in = "path",
                                description = "Nome do autor",
                                required = true,
                                schema = new { type = "string" }
                            }
                        },
                        responses = DefaultResponses()
                    }
                },
                ["/api/execute"] = new
                {
                    post = new
                    {
                        summary = "Executar ferramenta do MCP Server",
                        operationId = "ExecutarFerramenta",
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
                                            tool = new { type = "string" },
                                            parameters = new { type = "object" }
                                        }
                                    }
                                }
                            }
                        },
                        responses = DefaultResponses()
                    }
                },
                ["/api/ollama/generate"] = new
                {
                    post = new
                    {
                        summary = "Gerar texto com modelo Ollama",
                        operationId = "GerarTextoOllama",
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
                                            model = new { type = "string" },
                                            prompt = new { type = "string" }
                                        }
                                    }
                                }
                            }
                        },
                        responses = DefaultResponses()
                    }
                },
                ["/api/ollama/models"] = new
                {
                    get = new
                    {
                        summary = "Listar modelos disponíveis no Ollama",
                        operationId = "ListarModelosOllama",
                        responses = DefaultResponses()
                    }
                },
                ["/api/status"] = new
                {
                    get = new
                    {
                        summary = "Verificar status da integração",
                        operationId = "VerificarStatus",
                        responses = DefaultResponses()
                    }
                }
            };
        }

        private static Dictionary<string, object> DefaultResponses()
        {
            return new Dictionary<string, object>
            {
                ["200"] = new
                {
                    description = "Operação bem-sucedida",
                    content = new Dictionary<string, object>
                    {
                        ["application/json"] = new
                        {
                            schema = new { type = "object" }
                        }
                    }
                },
                ["400"] = new
                {
                    description = "Requisição inválida"
                },
                ["500"] = new
                {
                    description = "Erro interno do servidor"
                }
            };
        }
    }

    // Extensão para adicionar o middleware ao pipeline do ASP.NET Core
    public static class OpenApiMiddlewareExtensions
    {
        public static IApplicationBuilder UseOpenApiMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OpenApiMiddleware>();
        }
    }
}
