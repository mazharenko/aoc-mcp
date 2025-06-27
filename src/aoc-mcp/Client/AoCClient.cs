using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace mazharenko.aoc_mcp.Client;

internal class AoCClient(string sessionToken, IHttpClientFactory httpClientFactory, ILogger<AoCClient> logger) : IAoCClient
{
	private HttpClient CreateHttpClient()
	{
		var httpClient = httpClientFactory.CreateClient();
		httpClient.BaseAddress = new Uri("https://adventofcode.com");
		httpClient.DefaultRequestHeaders.Add("cookie", "session=" + sessionToken);
#pragma warning disable CS0618 // Type or member is obsolete
		const string version = ThisAssembly.Info.Version;
#pragma warning restore CS0618 // Type or member is obsolete
		httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"aoc-mcp/{version} (+via github.com/mazharenko/aoc-mcp by mazharenko.a@gmail.com)");
		return httpClient;
	}

	private static readonly Regex DayRegex = new(
		"""
		aria-label="Day (?<day>\d+)(, (?<stars>((one star)|(two stars))?))?"
		""");
	
	public async Task<SubmissionResult> SubmitAnswer(int year, int day, int part, string answer)
	{
		// todo try LLM sampling?
		logger.LogInformation("Submitting answer for Year {Year} Day {Day} Part {Part}", year, day, part);
		var form = new FormUrlEncodedContent(
			new Dictionary<string, string>
			{
				["level"] = part.ToString(),
				["answer"] = answer
			}
		);
		using var httpClient = CreateHttpClient();
		logger.LogDebug("Sending submission request to AoC");
		var response = await httpClient.PostAsync($"/{year}/day/{day}/answer", form);
		response.EnsureSuccessStatusCode();
		var content = await response.Content.ReadAsStringAsync();
		logger.LogDebug("Received submission response, length: {Length}", content.Length);
		if (content.Contains("That's the right answer!"))
		{
			logger.LogInformation("Submission for Year {Year} Day {Day} Part {Part} was correct", year, day, part);
			return new SubmissionResult.Correct();
		}
		if (content.Contains("too high"))
		{
			logger.LogInformation("Submission for Year {Year} Day {Day} Part {Part} was too high", year, day, part);
			return new SubmissionResult.TooHigh();
		}
		if (content.Contains("too low"))
		{
			logger.LogInformation("Submission for Year {Year} Day {Day} Part {Part} was too low", year, day, part);
			return new SubmissionResult.TooLow();
		}
		if (content.Contains("That's not the right answer"))
		{
			logger.LogInformation("Submission for Year {Year} Day {Day} Part {Part} was incorrect", year, day, part);
			return new SubmissionResult.Incorrect();
		}
		if (content.Contains("You don't seem to be solving the right level."))
		{
			logger.LogDebug("Level appears to be already solved, checking stats");
			if ((await GetDayResults(year)).IsSolved(day, part))
			{
				logger.LogInformation("Confirmed Year {Year} Day {Day} Part {Part} was already solved", year, day, part);
				return new SubmissionResult.Correct();
			}
			logger.LogError("Could not interpret submission result for Year {Year} Day {Day} Part {Part}", year, day, part);
			throw new InvalidOperationException("Could not interpret the submission result");
		}
		var timeLeftRegex = new Regex(@"You have ((?<min>\d+)m )?((?<sec>\d+)s )?left");
		var timeLeftMatch = timeLeftRegex.Match(content);
		if (timeLeftMatch.Success)
		{
			var min = timeLeftMatch.Groups["min"].Success
				? int.Parse(timeLeftMatch.Groups["min"].Value)
				: 0;
			var sec = timeLeftMatch.Groups["sec"].Success
				? int.Parse(timeLeftMatch.Groups["sec"].Value)
				: 0;
			var waitTime = new TimeSpan(0, min, sec);
			logger.LogInformation("Submission blocked - must wait {Minutes:00}:{Seconds:00} before next attempt", waitTime.Minutes, waitTime.Seconds);
			return new SubmissionResult.TooRecently(waitTime);
		}
		
		logger.LogError("Could not interpret submission result for Year {Year} Day {Day} Part {Part}", year, day, part);
		throw new InvalidOperationException("Could not interpret the submission result");
	}
	public async Task<Stats> GetDayResults(int year)
	{
		logger.LogInformation("Starting to collect AoC stats for year {Year}", year);
		var stats = new Stats();
		try
		{
			await foreach (var (day, part) in _GetDayResults(year))
			{
				stats.Solved(day, part);
				logger.LogDebug("Recorded star for Day {Day} Part {Part}", day, part);
			}
			logger.LogInformation("Completed collecting stats for year {Year}. Total stars: {Stars}", year, stats.Stars);
			return stats;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to collect stats for year {Year}", year);
			throw;
		}
	}
	
	private async IAsyncEnumerable<(int day, int part)> _GetDayResults(int year)
	{
		logger.LogInformation("Fetching AoC progress for year {Year}", year);
		
		using var httpClient = CreateHttpClient();
		string response;
		try 
		{
			response = await httpClient.GetStringAsync($"/{year}");
			logger.LogDebug("Received response from AoC for year {Year}, length: {Length}", year, response.Length);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to fetch AoC progress for year {Year}", year);
			throw;
		}
		
		var statParsed = DayRegex.Matches(response);
		logger.LogInformation("Found {Count} day matches in response", statParsed.Count);
		
		foreach (Match match in statParsed)
		{
			var day = int.Parse(match.Groups["day"].Value);
			var stars = match.Groups["stars"].Value;
			logger.LogDebug("Processing Day {Day}, stars: '{Stars}'", day, stars);
			
			switch (stars)
			{
				case "":
					logger.LogDebug("Day {Day}: No stars", day);
					break;
				case "one star":
					logger.LogDebug("Day {Day}: One star", day);
					yield return (day, 1);
					break;
				case "two stars":
					logger.LogDebug("Day {Day}: Two stars", day);
					yield return (day, 1);
					yield return (day, 2);
					break;
				default:
					logger.LogError("Could not interpret day results for Day {Day}: {Match}", day, match.Value);
					throw new InvalidOperationException($"Could not interpret day results for Day {day}: {match.Value}");
			}
		}
	}
}
