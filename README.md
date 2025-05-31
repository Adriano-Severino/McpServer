# McpServer

Um servidor de implementação do Model Context Protocol (MCP) para integração com modelos de IA e gerenciamento de livros.

## Visão Geral

O McpServer é uma solução completa para integrar modelos de IA do Ollama com uma API de gerenciamento de livros. O projeto implementa o protocolo MCP (Model Context Protocol) e oferece uma interface HTTP para execução de ferramentas via API.

O sistema é composto por três componentes principais:
- **McpServer**: Implementação do protocolo MCP em .NET
- **Livros API**: API para gerenciamento de dados de livros
- **Ollama**: Serviço para execução de modelos de IA

## Requisitos do Sistema

- Docker e Docker Compose
- .NET 9.0 (para desenvolvimento)
- Pelo menos 8GB de RAM (recomendado 16GB para modelos de IA maiores)
- 10GB de espaço em disco

## Instalação e Execução

### Método 1: Usando Docker Compose (Recomendado)

1. Clone o repositório:
   ```
   git clone <url-do-repositorio>
   cd McpServer
   ```

2. Execute o sistema usando Docker Compose:
   ```
   docker-compose up -d
   ```

3. Verifique se todos os serviços estão rodando:
   ```
   docker-compose ps
   ```

### Método 2: Executando localmente (para desenvolvimento)

1. Clone o repositório:
   ```
   git clone <url-do-repositorio>
   cd McpServer
   ```

2. Execute o Ollama localmente ou via Docker:
   ```
   docker run -d -p 11434:11434 -v ollama_data:/root/.ollama --name ollama ollama/ollama
   ```

3. Execute a API de Livros:
   ```
   cd Livros
   dotnet run
   ```

4. Configure as variáveis de ambiente para o McpServer:
   ```
   set OLLAMA_BASE_URL=http://localhost:11434
   set API_BASE_ADDRESS=http://localhost:5000/api/
   ```

5. Execute o McpServer:
   ```
   cd McpServer
   dotnet run
   ```

## Uso do Sistema

Uma vez que o sistema esteja em execução, você pode acessar:

### Endpoints da API

- **Servidor MCP**: http://localhost:5500
  - Lista de ferramentas: http://localhost:5500/api/tools
  - Documento OpenAPI: http://localhost:5500/api/openapi.json
  - Execução de ferramentas: http://localhost:5500/api/execute (POST)

- **API de Livros**: http://localhost:5000
  - API REST para gerenciamento de livros

### Integração com Open WebUI

Para integrar com o Open WebUI:

1. Inicie o Open WebUI (se não estiver usando o docker-compose completo):
   ```
   docker run -d -p 3000:8080 -e OLLAMA_BASE_URL=http://localhost:11434 -e MCP_SERVER_URL=http://localhost:5500 ghcr.io/open-webui/open-webui:cuda
   ```

2. Acesse o Open WebUI em: http://localhost:3000

3. Configure a conexão com o MCP Server:
   - URL: http://localhost:5500/api/openapi.json

## Ferramentas Disponíveis

O MCP Server oferece as seguintes ferramentas:

### Gerenciamento de Livros

- **ObterAsync**: Buscar livros com filtro opcional por título
- **ObterPorAutor**: Buscar livros com filtro por autor
- **CadastrarAsync**: Criar/cadastrar um novo livro
- **AtualizarAsync**: Atualizar dados de um livro existente

### Integração com Ollama

- **ListModels**: Lista os modelos disponíveis no Ollama
- **GenerateResponse**: Gera uma resposta usando um modelo do Ollama

### Utilidades do Sistema

- **VerificarConectividade**: Verifica a conectividade entre os componentes
- **ExibirConfiguracao**: Exibe as configurações de conexão do MCP Server

## Exemplos de Uso

### Exemplo 1: Listar Livros

```json
// POST para http://localhost:5500/api/execute
{
  "tool": "ObterAsync",
  "parameters": {
    "titulo": "O Senhor dos Anéis"
  }
}
```

### Exemplo 2: Cadastrar um Livro

```json
// POST para http://localhost:5500/api/execute
{
  "tool": "CadastrarAsync",
  "parameters": {
    "livro": {
      "titulo": "O Hobbit",
      "autor": "J.R.R. Tolkien",
      "anoPublicacao": 1937,
      "genero": "Fantasia"
    }
  }
}
```

### Exemplo 3: Gerar Resposta com Ollama

```json
// POST para http://localhost:5500/api/execute
{
  "tool": "GenerateResponse",
  "parameters": {
    "model": "llama3",
    "prompt": "Escreva um resumo sobre O Senhor dos Anéis"
  }
}
```

## Configuração

### Variáveis de Ambiente

- **OLLAMA_BASE_URL**: URL base para o serviço Ollama (padrão: http://ollama:11434)
- **API_BASE_ADDRESS**: Endereço base da API de Livros (padrão: http://livros-api:5000/api/)

## Solução de Problemas

### Problemas de Conectividade

Se encontrar problemas de conectividade entre os serviços:

1. Verifique se todos os contêineres estão em execução:
   ```
   docker-compose ps
   ```

2. Use a ferramenta de verificação de conectividade:
   ```json
   // POST para http://localhost:5500/api/execute
   {
     "tool": "VerificarConectividade",
     "parameters": {}
   }
   ```

3. Verifique os logs dos contêineres:
   ```
   docker-compose logs mcpserver
   docker-compose logs livros-api
   docker-compose logs ollama
   ```

### Problemas com Modelos Ollama

Se os modelos não estiverem carregando:

1. Verifique se o Ollama está em execução
2. Verifique se o modelo está disponível no Ollama:
   ```
   docker exec -it ollama ollama list
   ```
3. Baixe um modelo se necessário:
   ```
   docker exec -it ollama ollama pull llama3
   ```

## Arquitetura do Sistema

```
┌─────────────┐     ┌──────────────┐     ┌───────────┐
│  Open WebUI │────►│  MCP Server  │────►│  Ollama   │
└─────────────┘     └──────────────┘     └───────────┘
                           │
                           ▼
                    ┌──────────────┐
                    │  Livros API  │
                    └──────────────┘
```

## Desenvolvimento

Para contribuir com o projeto:

1. Faça um fork do repositório
2. Crie uma branch para sua feature (`git checkout -b feature/nome-da-feature`)
3. Faça commit das alterações (`git commit -am 'Adiciona funcionalidade X'`)
4. Faça push para a branch (`git push origin feature/nome-da-feature`)
5. Crie um Pull Request
