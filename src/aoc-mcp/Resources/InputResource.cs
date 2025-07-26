using System.ComponentModel;
using mazharenko.aoc_mcp.Client;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace mazharenko.aoc_mcp.Resources;

[McpServerResourceType]
public class InputResource
{
	[McpServerResource(UriTemplate = "aocinput://{year}/{day}", Name = "Puzzle input resource", MimeType = "text/plain")]
	[Description("Retrieves a puzzle input")]
	public static async Task<ResourceContents> GetInputResource(int year, int day,
		ILogger<InputResource> logger, IAoCClient client, CancellationToken cancellationToken)
	{
		try
		{
			var input = await client.LoadInput(year, day, cancellationToken);
			return new TextResourceContents{
				Text = input,
				MimeType = "text/plain",
				Uri = $"aocinput://{year}/{day}"
			};
		}
		catch (Exception ex)
		{
			throw new McpException(ex.Message, ex);
		}
	}
}