using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
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
		IMcpServer thisServer,
		CancellationToken cancellationToken)
	{
		try
		{
			var progressToken = context.Params?.ProgressToken;
			while (true)
			{
				var submissionText = await aocClient.SubmitAnswer(year, day, part, answer, cancellationToken);

				logger.LogInformation("Received submission result text: {SubmissionResultText}", submissionText);
				
				logger.LogInformation("Requesting a sample");
				
				var submissionSamplingResult = await thisServer.SampleAsync(new CreateMessageRequestParams
				{
					IncludeContext = ContextInclusion.None,
					Messages =
					[
						new SamplingMessage
						{
							Role = Role.Assistant,
							Content = new TextContentBlock
							{
								Text =
									$$"""
									The following text contains the result of an Advent of Code puzzle answer submission.
									Your task is to extract the verdict from the text. The verdict must be one of the following values:
									- "Correct"
									- "Incorrect"
									- "TooLow"
									- "TooHigh"
									- "TooRecently"
									- "CouldNotInterpret"
									Apply the following rules in this exact order:
									1. If the text states that the answer was correct / right → verdict is "Correct".
									2. If the text states the answer is too low or too high → verdict is "TooLow" or "TooHigh" respectively.
									3. If the text states that the answer was incorrect / not right → verdict is "Incorrect".
									4. If the text explicitly says the answer was given too recently, → verdict is "TooRecently".
									   - In this case, also extract the wait time from the text.
									   - Return the wait time in format hh:mm:ss (e.g., "00:05:00").
									   - It must be valid for .NET TimeSpan.
									5. If none of the above apply → verdict is "CouldNotInterpret".
									Return a JSON object:
									- For all verdicts except "TooRecently": `{ "verdict": "<value>" }`
									- For "TooRecently": `{ "verdict": "TooRecently", "toWait": "<hh:mm:ss>" }`
									///
									{{submissionText}}
									"""
							}
						}
					]
				}, cancellationToken);

				var submissionResult = (submissionSamplingResult.Content as TextContentBlock)?.Text;
				if (submissionResult is null)
					throw new Exception("Could not interpret the submission result, sampling result text is null");

				logger.LogInformation("Submission result is interpreted as {SubmissionResult}", submissionResult);

				var submissionResultObject = JsonSerializer.Deserialize<SubmissionSamplingResult>(submissionResult)!;
				
				logger.LogInformation("Submission result verdict is {SubmissionResultVerdict}", submissionResultObject.Verdict);
				
				switch (submissionResultObject.Verdict)
				{
					case "Correct":
						return SubmissionToolResult.Correct;
					case "Incorrect":
						return SubmissionToolResult.Incorrect;
					case "TooHigh":
						return SubmissionToolResult.TooHigh;
					case "TooLow":
						return SubmissionToolResult.TooLow;
					case "TooRecently":
						var toWait = submissionResultObject.ToWait!.Value;
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
						throw new ArgumentOutOfRangeException($"Unknown submission verdict {submissionResultObject.Verdict}");
				}
			}

		}
		catch (TaskCanceledException)
		{
			throw;
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

	private record SubmissionSamplingResult([property: JsonPropertyName("verdict")] string Verdict, [property: JsonPropertyName("toWait")]TimeSpan? ToWait);
}