using McpServer.DTOs;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace McpServer.Client
{
    public class OllamaClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public OllamaClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
        }

        public async Task<string> GenerateResponse(string model, string prompt)
        {
            try
            {
                // Cria o payload para a API do Ollama
                var requestBody = new
                {
                    model = model,
                    prompt = prompt,
                    stream = false
                };

                // Envia requisição para a API do Ollama
                var response = await _httpClient.PostAsJsonAsync("api/generate", requestBody);
                response.EnsureSuccessStatusCode();

                // Processa a resposta do Ollama
                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(_jsonOptions);
                return result?.Response ?? "Sem resposta do modelo";
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao comunicar com Ollama: {ex.Message}", ex);
            }
        }

        public async Task<string> ListModels()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/tags");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ModelsResponse>(_jsonOptions);
                if (result?.Models == null || !result.Models.Any())
                    return "Nenhum modelo encontrado";

                // Formata a lista de modelos como JSON
                return JsonSerializer.Serialize(result.Models.Select(m => new {
                    Nome = m.Name,
                    Tamanho = FormatSize(m.Size),
                    UltimoUso = m.ModifiedAt
                }), _jsonOptions);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao listar modelos: {ex.Message}", ex);
            }
        }

        public async Task<string> AnalyzeImage(string model, string imageData, string prompt)
        {
            try
            {
                // Verifica se a imagem é uma URL ou base64
                bool isUrl = Uri.TryCreate(imageData, UriKind.Absolute, out _);

                // Prepara o payload para a API
                var requestBody = new
                {
                    model = model,
                    prompt = prompt,
                    stream = false,
                    images = isUrl ? new[] { imageData } : new[] { Convert.ToBase64String(Encoding.UTF8.GetBytes(imageData)) }
                };

                // Envia requisição para a API do Ollama
                var response = await _httpClient.PostAsJsonAsync("api/generate", requestBody);
                response.EnsureSuccessStatusCode();

                // Processa a resposta do Ollama
                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(_jsonOptions);
                return result?.Response ?? "Sem resposta do modelo";
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao analisar imagem: {ex.Message}", ex);
            }
        }

        public async Task<string> GenerateWithParams(string model, string prompt, float temperature, float topP, int maxTokens)
        {
            try
            {
                // Cria o payload para a API do Ollama com parâmetros avançados
                var requestBody = new
                {
                    model = model,
                    prompt = prompt,
                    stream = false,
                    options = new
                    {
                        temperature,
                        top_p = topP,
                        num_predict = maxTokens
                    }
                };

                // Envia requisição para a API do Ollama
                var response = await _httpClient.PostAsJsonAsync("api/generate", requestBody);
                response.EnsureSuccessStatusCode();

                // Processa a resposta do Ollama
                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(_jsonOptions);
                return result?.Response ?? "Sem resposta do modelo";
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao gerar resposta com parâmetros: {ex.Message}", ex);
            }
        }

        public async Task<string> PullModel(string modelName)
        {
            try
            {
                var requestBody = new
                {
                    name = modelName
                };

                var response = await _httpClient.PostAsJsonAsync("api/pull", requestBody);
                response.EnsureSuccessStatusCode();

                // A operação de pull pode levar tempo, então retornamos uma mensagem inicial
                return $"Iniciado o download do modelo {modelName}. Este processo pode demorar dependendo do tamanho do modelo.";
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao baixar modelo: {ex.Message}", ex);
            }
        }

        public async Task<string> GenerateEmbeddings(string model, string text)
        {
            try
            {
                var requestBody = new
                {
                    model = model,
                    prompt = text,
                };

                var response = await _httpClient.PostAsJsonAsync("api/embeddings", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<EmbeddingsResponse>(_jsonOptions);
                if (result?.Embedding == null || !result.Embedding.Any())
                    return "Não foi possível gerar embeddings";

                // Retorna apenas os primeiros 5 valores para não sobrecarregar a resposta
                return $"Embedding gerado com {result.Embedding.Length} dimensões. Primeiros valores: [{string.Join(", ", result.Embedding.Take(5))}...]";
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao gerar embeddings: {ex.Message}", ex);
            }
        }

        private string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        private class ModelsResponse
        {
            public List<ModelInfo> Models { get; set; }
        }

        private class ModelInfo
        {
            public string Name { get; set; }
            public long Size { get; set; }
            public string ModifiedAt { get; set; }
        }

        private class EmbeddingsResponse
        {
            public float[] Embedding { get; set; }
        }
    }
}