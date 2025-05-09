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
            var url = string.IsNullOrWhiteSpace(titulo) ? "v1/Livros" : $"Livros?titulo={Uri.EscapeDataString(titulo)}";
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return new List<LivroResponse>();

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<LivroResponse>>(_jsonOptions);
        }

        public async Task<List<LivroResponse>> ObterPorAutorAsync(string? autor = null)
        {
            var response = await _httpClient.GetAsync($"v1/Livros/autor/{Uri.EscapeDataString(autor)}");

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent ||
                response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<LivroResponse>();

            response.EnsureSuccessStatusCode();
            var livros = await response.Content.ReadFromJsonAsync<List<LivroResponse>>(_jsonOptions);
            return livros ?? new List<LivroResponse>();
        }

        public async Task<int?> CriarAsync(LivroRequest livro)
        {
            var livroRequest = new
            {
                id = 0,
                titulo = livro.Titulo,
                autor = livro.Autor
            };

            var response = await _httpClient.PostAsJsonAsync("v1/Livros", livroRequest);

            if (!response.IsSuccessStatusCode)
                return null;

            // Tenta extrair o ID da resposta ou do Location header
            try
            {
                if (response.Headers.Location != null)
                {
                    var segments = response.Headers.Location.Segments;
                    if (segments.Length > 0 && int.TryParse(segments[segments.Length - 1], out int id))
                        return id;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<int>(content, _jsonOptions);
            }
            catch
            {
                return 1; // Retorna um valor padrão se não conseguir extrair o ID
            }
        }

        public async Task<bool> AtualizarAsync(int id, LivroRequest livro)
        {
            var livroRequest = new LivroRequest
            {
                Id = id,
                Titulo = livro.Titulo,
                Autor = livro.Autor
            };

            var response = await _httpClient.PutAsJsonAsync($"v1/Livros/{id}", livroRequest);
            return response.IsSuccessStatusCode;
        }
    }
}
