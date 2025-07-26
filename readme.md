# Advent of Code MCP Server

MCP server to interact with the Advent of Code website.

This project is developed for educational purposes. The server aims to simplify interactions with the Advent of Code website and enhance the puzzle-solving workflow. 

However, building a [self-submitting app](https://github.com/mazharenko/aoc-agent) around solutions will likely be the most efficient approach.

## Tools

### GetAocProgress
- Gets star count for a specified Advent of Code year
- Parameters: `year` (int)
- Returns: `Stars` (int)

### SubmitAnswer
- Submits an answer for an Advent of Code day part. If the previous answer has been given too recently, waits until it can be submitted (with progress notifications).
- Parameters: int year, int day, int part, string answer
- Returns: `Correct`/`Incorrect`/`TooLow`/`TooHigh`

### GetInputResource
- Retrieves the personal puzzle input 
- Parameters: int year, int day
- Returns: input text as an embedded resource

## Resources
### aocinput://{year}/{day}
Resource template for the personal puzzle input

## Use this MCP Server

[![Install with Docker in VS Code](https://img.shields.io/badge/VS_Code-install-0098FF?logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=aoc&config=%7B%22command%22%3A%22docker%22%2C%22args%22%3A%5B%22run%22%2C%22-i%22%2C%22--rm%22%2C%22-e%22%2C%22SESSION_COOKIE%22%2C%22ghcr.io%2Fmazharenko%2Faoc-mcp%22%5D%2C%22env%22%3A%7B%22SESSION_COOKIE%22%3A%22%3Csession%3E%22%7D%7D)

```json
{
  "mcpServers": {
    "aoc": {
      "command": "docker",
      "args": [
        "run",
        "-i",
        "--rm",
        "-e",
        "SESSION_COOKIE",
        "ghcr.io/mazharenko/aoc-mcp"
      ],
      "env": {
        "SESSION_COOKIE": "<session>"
      }
    }
  }
}
```

## Prompt examples

\> get aoc stats for 2024
 
\> submit "42" for 2024/10/1

\> get aoc input for 2024/10 and save it

\> Get aoc input for 2024/10 and write it to a new file. Run aoc program for year 2024 day 10, pass 2024 and 10 as positional arguments and the created file path as --input-file argument. Read the answers from the output and submit them.