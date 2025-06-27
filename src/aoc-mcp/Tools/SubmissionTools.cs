using System.ComponentModel;
using System.Diagnostics;
using mazharenko.aoc_mcp.Client;
using mazharenko.aoc_mcp.Extension;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace mazharenko.aoc_mcp.Tools;

[McpServerToolType]
public class SubmissionTools(IAoCClient aocClient, ILogger<SubmissionTools> logger, IMcpServer server)
{
	[McpServerTool(Destructive = true, Idempotent = false, OpenWorld = true, ReadOnly = false)]
	[Description("""
	Submit an answer for a part. Returns if the answer was correct or incorrect or too high or too low
	""")]
	public async Task<SubmissionToolResult> SubmitAnswer(int year, int day, int part, string answer, 
		RequestContext<CallToolRequestParams> context,
		CancellationToken cancellationToken)
	{
		try
		{
			var progressToken = context.Params?.ProgressToken;
			while (true)
			{
				var submissionResult = await aocClient.SubmitAnswer(year, day, part, answer);

				switch (submissionResult)
				{
					case SubmissionResult.Correct:
						return SubmissionToolResult.Correct;
					case SubmissionResult.Incorrect:
						return SubmissionToolResult.Incorrect;
					case SubmissionResult.TooHigh:
						return SubmissionToolResult.TooHigh; 
					case SubmissionResult.TooLow:
						return SubmissionToolResult.TooLow;
					case SubmissionResult.TooRecently(var toWait):
						var timeoutStopwatch = Stopwatch.StartNew();
						var leftToWait = toWait - timeoutStopwatch.Elapsed;
						while (leftToWait >= TimeSpan.Zero)
						{
							if (progressToken is not null)
							{
								await server.SendNotificationAsync("notifications/progress", new
								{
									Progress = timeoutStopwatch.Elapsed.TotalSeconds,
									Total = toWait.TotalSeconds,
									progressToken,
									Message = $"Answer given too recently. Waiting for {leftToWait.ToHumanReadable()} before another attempt"
								}, cancellationToken: cancellationToken);
							}
							await Task.Delay(leftToWait.TotalMilliseconds > 1000
								? 1000
								: (int)leftToWait.TotalMilliseconds, cancellationToken);
							leftToWait = toWait - timeoutStopwatch.Elapsed;
						}
						continue;
					default:
						throw new ArgumentOutOfRangeException(nameof(submissionResult));
				}
			}
			
		}
		catch (Exception ex)
		{
			throw new McpException(ex.Message, ex);
		}
	}

	public enum SubmissionToolResult
	{
		Correct, Incorrect, TooHigh, TooLow
	}
}