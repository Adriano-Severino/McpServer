using McpServer.Client;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace McpServer.Tools
{
    [McpServerToolType]
    public static class WebUITools
    {
        [McpServerTool, Description("Verifica a conectividade entre os sistemas (MCP Server, Ollama e API de Livros)")]
        public static async Task<string> VerificarConectividade(ApiClient apiClient, OllamaClient ollamaClient)
        {
            var resultado = new Dictionary<string, string>();
            
            try
            {
                // Testa conexão com a API de Livros
                var livros = await apiClient.ObterAsync();
                resultado["API_Livros"] = "Conectado com sucesso";
            }
            catch (Exception ex)
            {
                resultado["API_Livros"] = $"Erro: {ex.Message}";
            }
            
            try
            {
                // Testa conexão com Ollama
                var modelos = await ollamaClient.ListModels();
                resultado["Ollama"] = "Conectado com sucesso";
            }
            catch (Exception ex)
            {
                resultado["Ollama"] = $"Erro: {ex.Message}";
            }
            
            return JsonSerializer.Serialize(resultado, new JsonSerializerOptions { WriteIndented = true });
        }
        
        [McpServerTool, Description("Integração com a API de livros e o LLM do Ollama")]
        public static async Task<string> ConsultarLivrosComAI(
            ApiClient apiClient, 
            OllamaClient ollamaClient,
            [Description("Nome do modelo do Ollama a ser usado")] string modelo,
            [Description("Consulta ou pergunta sobre livros")] string consulta)
        {
            try
            {
                // Obtém livros da API
                var livros = await apiClient.ObterAsync();
                
                if (livros == null || livros.Count == 0)
                {
                    return "Não há livros cadastrados para consultar.";
                }
                
                // Prepara o contexto para o LLM
                var contexto = $"Informações sobre os livros disponíveis:\n";
                foreach (var livro in livros)
                {
                    contexto += $"ID: {livro.Id}, Título: {livro.Titulo}, Autor: {livro.Autor}\n";
                }
                
                // Envia consulta para o LLM
                var prompt = $"{contexto}\n\nConsulta do usuário: {consulta}\n\nResponda com base nas informações dos livros fornecidas acima:";
                var resposta = await ollamaClient.GenerateResponse(modelo, prompt);
                
                return resposta;
            }
            catch (Exception ex)
            {
                return $"Erro ao processar consulta: {ex.Message}";
            }
        }
        
        [McpServerTool, Description("Exibe configurações de conexão do MCP Server")]
        public static string ExibirConfiguracao()
        {
            var config = new
            {
                McpServer = new
                {
                    Versao = "1.0.0",
                    Nome = "DotNetMCPServer"
                },
                Conexoes = new
                {
                    ApiLivros = Environment.GetEnvironmentVariable("API_BASE_ADDRESS") ?? "http://host.docker.internal:5000/api/",
                    Ollama = Environment.GetEnvironmentVariable("OLLAMA_BASE_URL") ?? "http://host.docker.internal:11434",
                    WebUI = "http://localhost:3000" // Open WebUI rodando na porta 3000
                }
            };
            
            return JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
