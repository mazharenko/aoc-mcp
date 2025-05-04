using mazharenko.aoc_mcp.Client;
using ModelContextProtocol.Server;

namespace mazharenko.aoc_mcp.Tools;

[McpServerToolType]
public class StatTools
{
	[McpServerTool(Title = "Get Advent of Code progress summary")]
	public static async Task<ToolStats> GetAocProgress(int year, IMcpServer thisServer,
		IAoCClient aocClient, CancellationToken cancellationToken)
	{
		var stats = await aocClient.GetDayResults(year);
		return new ToolStats(stats.Stars);
	}
}

public record ToolStats(int Stars);