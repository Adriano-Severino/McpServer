using McpServer.Client;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace McpServer.Tools
{
    [McpServerToolType]
    public static class OllamaIntegrationTools
    {
        [McpServerTool, Description("Consulta livros e analisa o resultado com um modelo do Ollama")]
        public static async Task<string> ConsultarLivrosComIA(
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

        [McpServerTool, Description("Lista os modelos disponíveis no Ollama")]
        public static async Task<string> ListarModelosOllama(OllamaClient ollamaClient)
        {
            try
            {
                return await ollamaClient.ListModels();
            }
            catch (Exception ex)
            {
                return $"Erro ao listar modelos: {ex.Message}";
            }
        }

        [McpServerTool, Description("Gera texto com um modelo do Ollama")]
        public static async Task<string> GerarTexto(
            OllamaClient ollamaClient,
            [Description("Nome do modelo a ser usado")] string modelo,
            [Description("Texto do prompt ou pergunta")] string prompt)
        {
            try
            {
                return await ollamaClient.GenerateResponse(modelo, prompt);
            }
            catch (Exception ex)
            {
                return $"Erro ao gerar texto: {ex.Message}";
            }
        }

        [McpServerTool, Description("Integração avançada entre a API de Livros e modelos do Ollama")]
        public static async Task<string> RealizarTarefaCompleta(
            ApiClient apiClient,
            OllamaClient ollamaClient,
            [Description("Nome do modelo a ser usado")] string modelo,
            [Description("Descrição da tarefa a ser realizada")] string tarefa)
        {
            try
            {
                // Obtém livros da API
                var livros = await apiClient.ObterAsync();
                
                // Prepara contexto para o Ollama
                var contexto = new
                {
                    Livros = livros,
                    Tarefa = tarefa
                };

                var promptJson = JsonSerializer.Serialize(contexto);
                var promptCompleto = $"Você é um assistente especializado em literatura e gestão de biblioteca. " +
                                     $"Aqui está uma lista de livros em formato JSON: {promptJson}\n\n" +
                                     $"Com base nesses dados, realize a seguinte tarefa: {tarefa}";
                
                // Executa a tarefa no Ollama
                var resposta = await ollamaClient.GenerateResponse(modelo, promptCompleto);
                
                return resposta;
            }
            catch (Exception ex)
            {
                return $"Erro ao processar tarefa: {ex.Message}";
            }
        }

        [McpServerTool, Description("Baixa um modelo para o Ollama")]
        public static async Task<string> BaixarModelo(
            OllamaClient ollamaClient,
            [Description("Nome do modelo a baixar (ex: llama3, gemma:7b)")] string nomeModelo)
        {
            try
            {
                return await ollamaClient.PullModel(nomeModelo);
            }
            catch (Exception ex)
            {
                return $"Erro ao baixar modelo: {ex.Message}";
            }
        }
    }
}
