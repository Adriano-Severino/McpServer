using McpServer.Client;
using McpServer.DTOs;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace McpServer.Tools
{
    [McpServerToolType]
    public static class LivrosTools
    {
        [McpServerTool, Description("Buscar os livros da livraria, definindo um filtro opcional por titulo")]
        public static async Task<string> ObterAsync(ApiClient apiClient, [Description("Filtra opcional pelo titulo do livro")] string titulo)
        {
            try
            {
                var livros = await apiClient.ObterAsync(titulo);
                return livros.Count == 0
                    ? "Nenhum livro encontrado"
                    : JsonSerializer.Serialize(livros);
            }
            catch(Exception ex)
            {
                return $"Erro ao buscar livros: {ex.Message}";
            }
        }

        [McpServerTool, Description("Criar/Cadastrar um livro")]
        public static async Task<string> CadastrarAsync (ApiClient apiClient, [Description("dados para criação do livro")] LivroRequest livro )
        {
            try
            {
                var id = await apiClient.CriarAsync(livro);
                return id is null
                    ? "Não foi possivel cadastrar o livro"
                    : JsonSerializer.Serialize(livro);
            }
            catch (Exception ex)
            {
                return $"Erro ao cadastrar o livro: {ex.Message}";
            }
          
        }

        [McpServerTool, Description("Atualizar os dados de um livro")]
        public static async Task<string> AtualizarAsync(ApiClient apiClient, 
            [Description("Código ou identificador do livro")] int id,
            [Description("Dados para atualização ded um livro")] LivroRequest livro)
        {
            try
            {
                var sucesso = await apiClient.AtualizarAsync(id, livro);
                return sucesso
                    ? "Livro atualizado com sucesso"
                    : "Não foi possivel ataulizar o livro";
            }
            catch (Exception ex)
            {
                return $"Erro ao atualizar o livro: {ex.Message}";
            }
        }
    }
}