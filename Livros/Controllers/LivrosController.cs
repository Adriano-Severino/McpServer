using Livros.Models;
using Livros.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Livros.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LivrosController : Controller
    {
        public readonly ILivroServices _livroServices;

        public LivrosController(ILivroServices livroServices)
        {
            _livroServices = livroServices;
        }

        [HttpGet()]
        [Description("Busca todos os livros")]
        public async Task<IActionResult> GetLivros(string? titulo = null)
        {
            var livros = await _livroServices.GetLivrosAsync(titulo);
            return livros != null ? Ok(livros) : NotFound("Nenhum livro encontrado.");
        }

        [HttpGet("{id}")]
        [Description("Busca livro por id")]
        public async Task<IActionResult> GetLivroById(int id)
        {
            var livro = await _livroServices.GetLivroByIdAsync(id);
            return livro != null ? Ok(livro) : NotFound($"Livro com ID {id} não encontrado.");
        }

        [HttpGet("autor/{autor}")]
        [Description("Busca livro por autor")]
        public async Task<IActionResult> GetLivroByAutor(string autor)
        {
            var livro = await _livroServices.GetLivroByAutorAsync(autor);
            return livro != null ? Ok(livro) : NotFound($"Nenhum livro encontrado para o autor {autor}.");
        }

        [HttpPost]
        [Description("Adiciona um livro")]
        public async Task<IActionResult> AddLivro([FromBody] LivrosRequest livro)
        {
            if (livro == null)
            {
                return BadRequest("Dados do livro inválidos.");
            }

            await _livroServices.AddLivroAsync(livro);
            return CreatedAtAction(nameof(GetLivroById), new { id = livro.Id }, livro);
        }

        [HttpPut("{id}")]
        [Description("Atualiza um livro")]
        public async Task<IActionResult> UpdateLivro(int id, [FromBody] LivrosRequest livro)
        {
            if (livro == null || id != livro.Id)
            {
                return BadRequest("Dados do livro inválidos.");
            }
            var existingLivro = await _livroServices.GetLivroByIdAsync(id);
            if (existingLivro == null)
            {
                return NotFound($"Livro com ID {id} não encontrado.");
            }
            await _livroServices.UpdateLivroAsync(livro);
            return Ok(livro);
        }

        [HttpDelete("{id}")]
        [Description("Deleta um livro")]
        public async Task<IActionResult> DeleteLivro(int id)
        {
            var livro = await _livroServices.GetLivroByIdAsync(id);
            if (livro == null)
            {
                return NotFound($"Livro com ID {id} não encontrado.");
            }
            await _livroServices.DeleteLivroAsync(id);
            return Ok(id);
        }
    }
}
