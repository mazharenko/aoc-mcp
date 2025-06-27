using System.ComponentModel;
using mazharenko.aoc_mcp.Client;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace mazharenko.aoc_mcp.Tools;

[McpServerToolType]
public class StatTools
{
	[McpServerTool(Destructive = false, Idempotent = true, OpenWorld = true, ReadOnly = false)]
	[Description("""
	Get Advent of Code progress summary, namely, number of acquired stars.
	""")]
	public async Task<ToolStats> GetAocProgress(int year, 
		IAoCClient aocClient, ILogger<StatTools> logger,
		CancellationToken cancellationToken)
	{
		try
		{
			logger.LogInformation("Fetching AoC progress for year {Year}", year);
			
			var stats = await aocClient.GetDayResults(year);
			logger.LogInformation("Retrieved stats for year {Year}: {Stars} stars", year, stats.Stars);
			
			return new ToolStats(stats.Stars);
		}
		catch (Exception ex)
		{
			throw new McpException(ex.Message, ex);
		}
	}
}

public record ToolStats(int Stars);