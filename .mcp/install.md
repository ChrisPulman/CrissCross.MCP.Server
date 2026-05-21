# CrissCross MCP Server Install

## NuGet stdio configuration

```json
{
  "type": "stdio",
  "command": "dnx",
  "args": [
    "CP.CrissCross.Mcp.Server@0.*",
    "--yes"
  ]
}
```

## Source-run configuration

```json
{
  "type": "stdio",
  "command": "dotnet",
  "args": [
    "run",
    "--project",
    "/path/to/CrissCross.MCP.Server/src/CrissCross.McpServer/CrissCross.McpServer.csproj"
  ]
}
```
