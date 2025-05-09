FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["McpServer/McpServer.csproj", "McpServer/"]
COPY ["McpServer/", "McpServer/"]
WORKDIR "/src/McpServer"
RUN dotnet publish /t:PublishContainer

FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine
ENTRYPOINT ["dotnet", "McpServer.dll"]