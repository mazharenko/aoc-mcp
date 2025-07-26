using System.ComponentModel;
using mazharenko.aoc_mcp.Client;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace mazharenko.aoc_mcp.Tools;

[McpServerToolType]
public class InputTools
{
	[McpServerTool(Destructive = false, Idempotent = true, OpenWorld = true, ReadOnly = false)]
	[Description("Retrieves a puzzle input")]
	public static async Task<EmbeddedResourceBlock> GetInputResource(int year, int day,
		ILogger<InputTools> logger, IAoCClient client, CancellationToken cancellationToken)
	{
		try
		{
			var input = await client.LoadInput(year, day, cancellationToken);

			return new EmbeddedResourceBlock
			{
				Resource = new TextResourceContents
				{
					MimeType = "text/plain",
					Uri = $"aocinput://{year}/{day}",
					Text = input
				}
			};
		}
		catch (Exception ex)
		{
			throw new McpException(ex.Message, ex);
		}
	}
}