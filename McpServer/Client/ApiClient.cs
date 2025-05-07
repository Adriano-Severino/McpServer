using McpServer.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace McpServer.Client
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
        }

        public async Task<List<LivroResponse>> ObterAsync(string? titulo = null)
        {
            var url = string.IsNullOrWhiteSpace(titulo) ? "livros" : $"livros?titulo={Uri.EscapeUriString(titulo)}";
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return new List<LivroResponse>();

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<LivroResponse>>(_jsonOptions);
        }

        public async Task<int?> CriarAsync(LivroRequest livro)
        {
            var response = await _httpClient.PostAsJsonAsync("livros", livro);

            if (!response.IsSuccessStatusCode)
                return null;

           var id = await response.Content.ReadFromJsonAsync<int>(_jsonOptions);
            return id;
        }

        public async Task<bool> AtualizarAsync(int id, LivroRequest livro)
        {
            var response = await _httpClient.PutAsJsonAsync($"livros/{id}", livro);

          return response.IsSuccessStatusCode;
        }
    }
}